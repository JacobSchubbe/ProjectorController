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
    
    connection.on('ReceiveQueryResponse', (response:QueryResponse) => {
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
    sendSystemQuery(consts.SystemControl.SourceQuery);
    console.log("Sent source query")
}






export const sendSystemCommand = (command:consts.SystemControl) => {
    console.log(`Sending ${command}`);
    if (connection) {
        connection.invoke('ReceiveSystemCommand', command)
            .catch(err => console.error('SignalR send error: ', err));
    }
};
export const sendSystemQuery = (command:consts.SystemControl) => {
    console.log(`Sending ${command}`);
    if (connection) {
        connection.invoke('ReceiveSystemQuery', command)
            .catch(err => console.error('SignalR send error: ', err));
    }
};
