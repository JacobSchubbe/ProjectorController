import * as signalR from '@microsoft/signalr';
import * as consts from './TcpConsts';
import { HubConnectionState } from '@microsoft/signalr';

let connection:signalR.HubConnection;

export type Message = {
    message: string;
}

export type QueryResponse = {
    queryType:Number;
    currentStatus:Number;
}

type OnMessageReceived = (message: Message) => void;
type IsConnectedToProjector = (isConnected:boolean) => void;
type OnQueryResponseReceived = (response:QueryResponse) => void;
type ConnectionStatusUpdater = (isConnected: boolean) => void;

export const initializeSignalR = async (
    isConnectedToProject:IsConnectedToProjector, 
    onQueryResponseReceived:OnQueryResponseReceived,
    connectionStatusUpdater:ConnectionStatusUpdater
) => {
    console.log('Environment variables:', process.env);
    const apiUrl = process.env.VUE_APP_API_URL || '192.168.0.153';
    console.log(`Connection API URL: ${apiUrl}`);
    connection = new signalR.HubConnectionBuilder()
        .withUrl(`http://${apiUrl}:19521/GUIHub`)
        .build();

    connection.on('IsConnectedToProjector', (isConnected:boolean) => {
        if (isConnectedToProject) {
            isConnectedToProject(isConnected);
        }
    });
    connection.on('ReceiveProjectorQueryResponse', (response:QueryResponse) => {
        console.log(`QueryType: ${response.queryType}, Current Status: ${response.currentStatus}`);
        if (onQueryResponseReceived) {
            onQueryResponseReceived(response);
        }
    });

    connection.onclose(async () => {
        console.log('SignalR connection closed');
        connectionStatusUpdater(false);
        restartSignalR(connectionStatusUpdater);
    });

    restartSignalR(connectionStatusUpdater);
};

const restartSignalR = async (connectionStatusUpdater:ConnectionStatusUpdater) => {
    while (connection.state !== HubConnectionState.Connected) {
        try {
            console.log("Attempting to connect to SignalR...");
            await connection.start();
            console.log('SignalR connected');
            connectionStatusUpdater(true);
            queryForInitialBackendStatuses();
        }
        catch (err) {
            if (err instanceof Error) {
                console.error(`SignalR connection error. Retrying connection again in 2 seconds. Error: ${err.message}`);
            } else {
                console.error(`SignalR connection error. Retrying connection again in 2 seconds. Unknown error: ${JSON.stringify(err)}`);
            }
            await new Promise((resolve) => setTimeout(resolve, 2000));
        }
    }
}

const queryForInitialBackendStatuses = () => {
    getIsConnectedToProjector();
    sendProjectorQuery(consts.ProjectorCommands.SystemControlPowerQuery)
}

export const queryForInitialProjectorStatuses = () => {
    sendProjectorQuery(consts.ProjectorCommands.SystemControlSourceQuery);
}

export const getIsConnectedToProjector = () => {
    console.log(`Get connection status to projector`);
    if (connection) {
        connection.invoke('IsConnectedToProjectorQuery')
            .catch(err => console.error('SignalR send error: ', err));
    }
};
export const sendProjectorCommands = (command:consts.ProjectorCommands) => {
    console.log(`Sending ${command}`);
    if (connection) {
        connection.invoke('ReceiveProjectorCommand', command)
            .catch(err => console.error('SignalR send error: ', err));
    }
};
export const sendProjectorQuery = (command:consts.ProjectorCommands) => {
    console.log(`Sending ${command}`);
    if (connection) {
        connection.invoke('ReceiveProjectorQuery', command)
            .catch(err => console.error('SignalR send error: ', err));
    }
};
