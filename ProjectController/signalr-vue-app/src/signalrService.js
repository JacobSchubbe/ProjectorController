import * as signalR from '@microsoft/signalr';

let connection;

export const initializeSignalR = (onMessageReceived) => {
    connection = new signalR.HubConnectionBuilder()
        .withUrl('http://localhost:5000/GUIHub') // Replace with your backend's URL
        .withAutomaticReconnect()
        .build();

    // Start the connection
    connection.start()
        .then(() => console.log('SignalR connected'))
        .catch(err => console.error('SignalR connection error: ', err));

    // Listen for the ReceiveMessage event
    connection.on('ReceiveMessage', (user, message) => {
        console.log(`Message from ${user}: ${message}`);
        if (onMessageReceived) {
            onMessageReceived(user, message); // Trigger callback if provided
        }
    });
};

export const sendMessage = (message) => {
    if (connection) {
        connection.invoke('SendMessage', 'VueUser', message)
            .catch(err => console.error('SignalR send error: ', err));
    }
};
