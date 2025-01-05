import * as signalR from '@microsoft/signalr';
import * as consts from './TcpConsts';

let connection:signalR.HubConnection;

export const initializeSignalR = (onMessageReceived:any) => {
    connection = new signalR.HubConnectionBuilder()
        .withUrl('http://localhost:5000/GUIHub') // Replace with your backend's URL
        .withAutomaticReconnect()
        .build();


    // Start listening for events
    connection.on('ReceiveMessage', (message) => {
        console.log(`Message: ${message}`);
        if (onMessageReceived) {
            onMessageReceived(message); // Trigger callback if provided
        }
    });


    // Start the connection
    connection.start()
        .then(() => console.log('SignalR connected'))
        .catch(err => console.error('SignalR connection error: ', err));
};

export const sendSystemCommand = (command:consts.SystemControl) => {
    if (connection) {
        connection.invoke('ReceiveSystemCommand', command)
            .catch(err => console.error('SignalR send error: ', err));
    }
};
