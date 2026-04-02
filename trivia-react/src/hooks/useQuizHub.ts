import { useEffect, useRef, useState, useCallback } from 'react';
import * as signalR from '@microsoft/signalr';

const HUB_URL = `${process.env.REACT_APP_API_URL}/hubs/quiz`;

// Matches your existing QuestionDto / AnswerDto from Entities.cs
export interface AnswerOption {
    id: number;
    answerText: string;
    isCorrect?: boolean; // only present on host side
}

export interface QuestionPayload {
    id: number;
    questionText: string;
    mediaUrl?: string;
    mediaType?: string;
    answers: AnswerOption[];
}

export interface AnswerRevealedPayload {
    correctAnswerId: number;
    totalAnswers: number;
    correctCount: number;
    results: {
        participantName: string;
        selectedAnswerId: number;
        isCorrect: boolean;
        submittedAt: string;
    }[];
    scoreboard: { displayName: string; score: number }[];
}

export function useQuizHub() {
    const connectionRef = useRef<signalR.HubConnection | null>(null);
    const [connected, setConnected] = useState(false);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
    const token = localStorage.getItem('token');
    
    if (!token) {
        setError('No auth token');
        return;
    }

    const connection = new signalR.HubConnectionBuilder()
        .withUrl(HUB_URL, {
            accessTokenFactory: () => localStorage.getItem('token') || '',
        })
        .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
        .configureLogging(signalR.LogLevel.Information)
        .build();

    connectionRef.current = connection;

    connection
        .start()
        .then(() => setConnected(true))
        .catch((err) => setError(err.message));

    connection.onreconnecting(() => setConnected(false));
    connection.onreconnected(() => setConnected(true));
    connection.onclose(() => setConnected(false));

    return () => {
        connection.stop();
    };
}, []); 

    const invoke = useCallback(
        async <T = void>(method: string, ...args: unknown[]): Promise<T> => {
            const conn = connectionRef.current;
            if (!conn || conn.state !== signalR.HubConnectionState.Connected) {
                throw new Error('Not connected to quiz hub');
            }
            return conn.invoke<T>(method, ...args);
        },
        []
    );

    const on = useCallback(
        (event: string, handler: (...args: any[]) => void) => {
            connectionRef.current?.on(event, handler);
            return () => connectionRef.current?.off(event, handler);
        },
        []
    );

    return { connected, error, invoke, on };
}