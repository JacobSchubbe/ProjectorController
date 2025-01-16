import * as signalR from '@microsoft/signalr';
import * as projectorConstants from "./Constants/ProjectorConstants";
import * as adbConstants from "./Constants/AdbConstants";
import { HubConnectionState } from '@microsoft/signalr';

// Types for messages/events
export type Message = {
    message: string;
};

export type QueryResponse = {
    queryType: number;
    currentStatus: number;
};

type OnMessageReceived = (message: Message) => void;
type IsConnectedToProjectorCallback = (isConnected: boolean) => void;
type OnQueryResponseReceived = (response: QueryResponse) => void;
type ConnectionStatusUpdater = (isConnected: boolean) => void;

// Configuration
const RETRY_INTERVAL_MS = 2000;
const DEFAULT_API_URL = process.env.VUE_APP_API_URL || '192.168.0.153';
const SIGNALR_URL = `http://${DEFAULT_API_URL}:19521/GUIHub`;

// SignalR Manager Class -------------------------------------------------
class SignalRService {
    private connection: signalR.HubConnection;
    private isConnectedToProjectorCallback?: IsConnectedToProjectorCallback;
    private onQueryResponseReceivedCallback?: OnQueryResponseReceived;
    private connectionStatusUpdater?: ConnectionStatusUpdater;

    constructor() {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(SIGNALR_URL)
            .build();
    }

    // Initialize the SignalR connection and listeners
    async initialize(
        isConnectedToProjector: IsConnectedToProjectorCallback,
        onQueryResponseReceived: OnQueryResponseReceived,
        connectionStatusUpdater: ConnectionStatusUpdater) 
    {
        console.log('Setting up SignalR Service...');

        this.isConnectedToProjectorCallback = isConnectedToProjector;
        this.onQueryResponseReceivedCallback = onQueryResponseReceived;
        this.connectionStatusUpdater = connectionStatusUpdater;

        // Register listeners
        this.connection.on('IsConnectedToProjector', this.handleConnectionStatus);
        this.connection.on('ReceiveProjectorQueryResponse', this.handleQueryResponse);

        this.connection.onclose(async () => {
            console.log('SignalR connection closed. Attempting to reconnect...');
            this.connectionStatusUpdater?.(false);
            await this.retryConnection();
        });

        await this.retryConnection(); // Start connection
    }

    // Teardown the current connection
    async disconnect(): Promise<void> {
        if (this.connection.state === HubConnectionState.Connected) {
            await this.connection.stop();
            console.log('SignalR connection stopped');
        }
    }

    // Query for initial statuses after connection
    queryForInitialBackendStatuses(): void {
        this.getIsConnectedToProjector();
        this.sendProjectorQuery(projectorConstants.ProjectorCommands.SystemControlPowerQuery);
    }

    // Restart SignalR connection in case of failure
    private async retryConnection(): Promise<void> {
        while (this.connection.state !== HubConnectionState.Connected) {
            try {
                console.log('Attempting to connect to SignalR...');
                await this.connection.start();
                console.log('SignalR connected');
                this.connectionStatusUpdater?.(true);
                this.queryForInitialBackendStatuses();
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
    private handleConnectionStatus = (isConnected: boolean): void => {
        console.log(`Projector connected status: ${isConnected}`);
        this.isConnectedToProjectorCallback?.(isConnected);
    };

    private handleQueryResponse = (response: QueryResponse): void => {
        console.log(`Query response received: ${JSON.stringify(response)}`);
        this.onQueryResponseReceivedCallback?.(response);
    };

    // Command functions
    getIsConnectedToProjector(): void {
        console.log('Querying connection status to projector...');
        this.invoke('IsConnectedToProjectorQuery');
    }

    sendProjectorCommand(command: projectorConstants.ProjectorCommands): void {
        console.log(`Sending projector command: ${command}`);
        this.invoke('ReceiveProjectorCommand', command);
    }

    sendProjectorQuery(command: projectorConstants.ProjectorCommands): void {
        console.log(`Sending projector query: ${command}`);
        this.invoke('ReceiveProjectorQuery', command);
    }
    
    sendAndroidCommand(command: adbConstants.KeyCodes): void {
        console.log(`Sending projector command: ${command}`);
        this.invoke('ReceiveAndroidCommand', command);
    }

    sendAndroidQuery(command: adbConstants.KeyCodes): void {
        console.log(`Sending projector query: ${command}`);
        this.invoke('ReceiveAndroidQuery', command);
    }

    queryForInitialProjectorStatuses = () => {
        this.sendProjectorQuery(projectorConstants.ProjectorCommands.SystemControlSourceQuery);
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