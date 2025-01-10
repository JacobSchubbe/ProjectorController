import * as signalR from '@microsoft/signalr';
import * as consts from './TcpConsts';

let connection:signalR.HubConnection;

export type Message = {
    message: string;
}

export type QueryResponse = {
    queryType:Number;
    currentStatus:Number;
}

type OnMessageReceived = (message: Message) => void;
type OnQueryResponseReceived = (response:QueryResponse) => void;
type ConnectionStatusUpdater = (isConnected: boolean) => void;

export const initializeSignalR = (
    onMessageReceived:OnMessageReceived, 
    onQueryResponseReceived:OnQueryResponseReceived,
    connectionStatusUpdater:ConnectionStatusUpdater
) => {
    console.log('Environment variables:', process.env);
    const apiUrl = process.env.VUE_APP_API_URL || '192.168.0.153';
    console.log(`Connection API URL: ${apiUrl}`);
    connection = new signalR.HubConnectionBuilder()
        .withUrl(`http://${apiUrl}:19521/GUIHub`)
        .withAutomaticReconnect()
        .build();
    
    connection.on('ReceiveMessage', (message:Message) => {
        if (onMessageReceived) {
            onMessageReceived(message);
        }
    });
    
    connection.on('ReceiveProjectorQueryResponse', (response:QueryResponse) => {
        console.log(`QueryType: ${response.queryType}, Current Status: ${response.currentStatus}`);
        if (onQueryResponseReceived) {
            onQueryResponseReceived(response);
        }
    });

    reconnectToSignalR(connectionStatusUpdater);
};

const reconnectToSignalR = async (connectionStatusUpdater:any) => {
    connection.start()
        .then(() => {
            console.log('SignalR connected');
            connectionStatusUpdater(true);
            queryForInitialStatuses()
        })
        .catch(err => console.error('SignalR connection error: ', err));
}






const queryForInitialStatuses = () => {
    console.log("Sending source query")
    sendProjectorQuery(consts.ProjectorCommands.SystemControlSourceQuery);
    console.log("Sent source query")
}


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
