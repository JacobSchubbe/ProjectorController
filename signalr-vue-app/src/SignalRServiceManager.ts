import * as signalR from '@microsoft/signalr';
import * as projectorConstants from "./Constants/ProjectorConstants";
import * as adbConstants from "./Constants/AdbConstants";
import * as tvConstants from "./Constants/TVConstants";
import { HubConnectionState } from '@microsoft/signalr';

// Types for messages/events
export type Message = {
    message: string;
};

export type QueryResponse = {
    queryType: number;
    currentStatus: number;
};

type IsConnectedToProjectorCallback = (isConnected: boolean) => void;
type IsConnectedToAndroidTVCallback = (isConnected: boolean) => void;
type OnHandleHdmiInputQuery = (response: Number) => void;
type OnAndroidTVQuery = (response: QueryResponse) => void;
type OnQueryResponseReceived = (response: QueryResponse) => void;
type ConnectionStatusUpdater = (isConnected: boolean) => void;

// Configuration
const RETRY_INTERVAL_MS = 2000;
const DEFAULT_API_URL = process.env.VUE_APP_API_URL || '192.168.0.153';
const SIGNALR_URL = `http://${DEFAULT_API_URL}:19521/GUIHub`;

// SignalR Manager Class -------------------------------------------------
class SignalRService {
    private connection: signalR.HubConnection;
    private pingInterval: number | undefined; // Ping interval ID

    private isConnectedToProjectorCallback?: IsConnectedToProjectorCallback;
    private isConnectedToAndroidTVCallback?: IsConnectedToAndroidTVCallback;
    private onHandleHdmiInputQueryCallback?: OnHandleHdmiInputQuery;
    private onHandleAndroidTVQueryCallback?: OnAndroidTVQuery;
    private onProjectorQueryResponseReceivedCallback?: OnQueryResponseReceived;
    private connectionStatusUpdater?: ConnectionStatusUpdater;

    constructor() {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(SIGNALR_URL)
            .build();
    }
    
    isConnected(): boolean {
        return !this.connection || this.connection.state === HubConnectionState.Connected;
    }

    // Initialize the SignalR connection and listeners
    async initialize(
        isConnectedToProjectorCallback: IsConnectedToProjectorCallback,
        isConnectedToAndroidTVCallback: IsConnectedToAndroidTVCallback,
        onHandleHdmiInputQuery: OnHandleHdmiInputQuery,
        onHandleAndroidTVQuery: OnAndroidTVQuery,
        onProjectorQueryResponseReceivedCallback: OnQueryResponseReceived,
        connectionStatusUpdater: ConnectionStatusUpdater) 
    {
        console.log('Setting up SignalR Service...');

        this.isConnectedToProjectorCallback = isConnectedToProjectorCallback;
        this.isConnectedToAndroidTVCallback = isConnectedToAndroidTVCallback;
        this.onHandleHdmiInputQueryCallback = onHandleHdmiInputQuery;
        this.onHandleAndroidTVQueryCallback = onHandleAndroidTVQuery;
        this.onProjectorQueryResponseReceivedCallback = onProjectorQueryResponseReceivedCallback;
        this.connectionStatusUpdater = connectionStatusUpdater;

        // Register listeners
        this.connection.on('ReceivedPing', this.handlePing);
        this.connection.on('IsConnectedToProjector', this.handleProjectorConnectionStatus);
        this.connection.on('IsConnectedToAndroidTVQuery', this.handleAndroidTVConnectionStatus);
        this.connection.on('ReceiveProjectorQueryResponse', this.handleProjectorQueryResponse);
        this.connection.on('ReceiveHdmiInputQueryResponse', this.handleHdmiQueryResponse);
        this.connection.on('ReceiveAndroidTVQueryResponse', this.handleAndroidTVQueryResponse);

        this.connection.onclose(async () => {
            console.log('SignalR connection closed. Attempting to reconnect...');
            this.connectionStatusUpdater?.(false);
            if (this.pingInterval) {
                clearInterval(this.pingInterval); // Stop the ping interval if connection is lost
                this.pingInterval = undefined;
            }
            await this.retryConnection();
        });

        document.addEventListener("visibilitychange", async () => {
            if (document.visibilityState === "visible" && this.connection.state !== HubConnectionState.Connected) {
                console.log("Browser became visible. Reconnecting SignalR...");
                await this.retryConnection(); // Reconnect if not already connected
            }
        });

        await this.retryConnection(); // Start connection
    }

    private startPing(): void {
        if (this.pingInterval) {
            clearInterval(this.pingInterval);
        }

        this.pingInterval = window.setInterval(() => {
            if (this.connection.state === HubConnectionState.Connected) {
                // console.log("Sending ping to keep the SignalR connection alive...");
                this.connection.send("Ping").catch(err => {
                    console.error("Error while sending ping:", err);
                });
            }
        }, 25000);
    }

    // Teardown the current connection
    async disconnect(): Promise<void> {
        if (this.connection.state === HubConnectionState.Connected) {
            await this.connection.stop();
            console.log('SignalR connection stopped');
        }

        if (this.pingInterval) {
            clearInterval(this.pingInterval);
            this.pingInterval = undefined;
        }
    }

    // Query for initial statuses after connection
    queryForInitialConnectionStatuses(): void {
        this.getIsConnectedToProjector();
        this.getIsConnectedToAndroidTV();
    }

    queryForInitialProjectorStatuses = () => {
        this.sendProjectorQuery(projectorConstants.ProjectorCommands.SystemControlSourceQuery);
        this.sendProjectorQuery(projectorConstants.ProjectorCommands.SystemControlVolumeQuery);
    }
    
    queryForInitialHdmiStatuses = () => {
        this.sendHdmiQuery()
    }

    queryForInitialAndroidTVStatuses = () => {
        this.sendAndroidQuery(adbConstants.KeyCodes.VpnStatusQuery)
    }
    
    // Restart SignalR connection in case of failure
    private async retryConnection(): Promise<void> {
        while (this.connection.state !== HubConnectionState.Connected) {
            try {
                console.log('Attempting to connect to SignalR...');
                await this.connection.start();
                console.log('SignalR connected');
                this.connectionStatusUpdater?.(true);
                this.queryForInitialConnectionStatuses();
                this.startPing(); // Start pings after successfully connecting to the server
            } catch (err) {
                this.logConnectionError(err);
                await this.sleep(RETRY_INTERVAL_MS);
            }
        }
    } 

    // Utility: Sleep for retry interval
    private sleep(ms: number): Promise<void> {
        return new Promise(resolve => setTimeout(resolve, ms));
    }

    // Event handlers
    private handlePing = (): void => {
        // console.log(`Received reply from server for ping.`);
    };
    
    private handleProjectorConnectionStatus = (isConnected: boolean): void => {
        console.log(`Projector connected status: ${isConnected}`);
        this.isConnectedToProjectorCallback?.(isConnected);
    };

    private handleHdmiQueryResponse = (input: Number): void => {
        console.log(`Received hdmi input query response: ${input}`);
        this.onHandleHdmiInputQueryCallback?.(input);
    }
    
    private handleAndroidTVQueryResponse = (response: QueryResponse): void => {
        console.log(`Received AndroidTV query response: ${JSON.stringify(response)}`);
        this.onHandleAndroidTVQueryCallback?.(response);
    }
    
    private handleAndroidTVConnectionStatus = (isConnected: boolean): void => {
        console.log(`AndroidTV connected status: ${isConnected}`);
        this.isConnectedToAndroidTVCallback?.(isConnected);
    };

    private handleProjectorQueryResponse = (response: QueryResponse): void => {
        console.log(`Projector query response received: ${JSON.stringify(response)}`);
        this.onProjectorQueryResponseReceivedCallback?.(response);
    };

    // Command functions
    getIsConnectedToProjector(): void {
        console.log('Querying connection status to projector...');
        this.invoke('IsConnectedToProjectorQuery');
    }
    
    getIsConnectedToAndroidTV(): void {
        console.log('Querying connection status to AndroidTV...');
        this.invoke('IsConnectedToAndroidTVQuery');
    }

    sendProjectorVolumeSet(volume: number): void {
        console.log(`Sending projector volume set: ${volume}`);
        this.invoke('ReceiveProjectorVolumeSet', volume);
    }
    
    sendProjectorCommand(command: projectorConstants.ProjectorCommands): void {
        console.log(`Sending projector command: ${command}`);
        this.invoke('ReceiveProjectorCommand', command);
    }

    sendHdmiCommand(input: projectorConstants.ProjectorCommands): void {
        console.log(`Sending hdmi input: ${input}`);
        this.invoke('ReceiveHdmiInput', input);
    }

    sendHdmiQuery(): void {
        console.log(`Sending hdmi input query`);
        this.invoke('ReceiveHdmiInputQuery');
    }

    sendProjectorQuery(command: projectorConstants.ProjectorCommands): void {
        console.log(`Sending projector query: ${command}`);
        this.invoke('ReceiveProjectorQuery', command);
    }
    
    sendTVCommand(command: tvConstants.IRCommands): void {
        console.log(`Sending TV command: ${command}`);
        this.invoke('ReceiveTVCommand', command);
    }

    sendAndroidCommand(command: adbConstants.KeyCodes, isLongPress: boolean): void {
        console.log(`Sending AndroidTV command: ${command} with long press: ${isLongPress}`);
        this.invoke('ReceiveAndroidCommand', command, isLongPress);
    }

    sendAndroidOpenAppCommand(command: adbConstants.KeyCodes): void {
        console.log(`Sending AndroidTV open app command: ${command}`);
        this.invoke('ReceiveAndroidOpenAppCommand', command);
    }

    sendAndroidQuery(command: adbConstants.KeyCodes): void {
        console.log(`Sending AndroidTV query: ${command}`);
        this.invoke('ReceiveAndroidQuery', command);
    }
    
    // Helper to send SignalR invocations
    private invoke(methodName: string, ...args: any[]): void {
        if (this.connection.state === HubConnectionState.Connected) {
            this.connection.invoke(methodName, ...args).catch(err =>
                console.error(`SignalR invocation error for method '${methodName}': ${err}`)
            );
        } else {
            console.error(`Cannot invoke method '${methodName}' because the connection is not established.`);
        }
    }

    // Error logging
    private logConnectionError(err: any): void {
        if (err instanceof Error) {
            console.error(`SignalR connection error: ${err.message}`);
        } else {
            console.error(`SignalR connection error: ${JSON.stringify(err)}`);
        }
    }
}

// Exported instance for use in the app
export const SignalRInstance = new SignalRService();