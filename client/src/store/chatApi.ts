import { createApi } from '@reduxjs/toolkit/query/react';
import { baseQueryWithReauth } from './baseQuery';

export interface ChatMessage {
    message: string;
}

export interface ChatResponse {
    response: string;
    model: string;
}

export const chatApi = createApi({
    reducerPath: 'chatApi',
    baseQuery: baseQueryWithReauth,
    tagTypes: ['Chat'],
    endpoints: (builder) => ({
        sendMessage: builder.mutation<ChatResponse, ChatMessage>({
            query: (message) => ({
                url: '/chat/message',
                method: 'POST',
                body: message,
            }),
        }),
        getChatHistory: builder.query<ChatMessage[], void>({
            query: () => '/chat/history',
            providesTags: ['Chat'],
        }),
    }),
});

export const { useSendMessageMutation, useGetChatHistoryQuery } = chatApi;
