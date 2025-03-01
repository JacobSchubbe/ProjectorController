import axios, { AxiosError } from 'axios';

const originalConsoleInfo = console.info;
const originalConsoleDebug = console.log;
const originalConsoleWarn = console.warn;
const originalConsoleError = console.error;

function sendLogToServer(level: string, message: string) {
    if (process.env.VUE_APP_PORT){
        axios.post('/api/logs', { level, message }).catch((err) => {
            originalConsoleError('Failed to send log to server:', err);
        });
    }
}

// Override console.log
console.info = (...args: any[]) => {
    const message = args.join(' ');
    originalConsoleInfo(...args); // Still log to the browser console
    sendLogToServer('info', message); // Send the log to the server (level: info)
};

console.log = (...args: any[]) => {
    const message = args.join(' ');
    originalConsoleDebug(...args); // Still log to the browser console
    sendLogToServer('debug', message); // Send the log to the server (level: info)
};

console.warn = (...args: any[]) => {
    const message = args.join(' ');
    originalConsoleWarn(...args); // Still log to the browser console
    sendLogToServer('warning', message); // Send the log to the server (level: info)
}

// Override console.error
console.error = (...args: any[]) => {
    const message = args.join(' ');
    originalConsoleError(...args);
    sendLogToServer('error', message); // Send the error to the server (level: error)
};

// You can override other functions like console.warn or console.debug similarly.