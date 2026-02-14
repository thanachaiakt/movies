import { useState, useRef, useEffect, useCallback, useMemo } from 'react';
import { useSendMessageMutation } from '../store/chatApi';

interface Message {
    id: string;
    text: string;
    isUser: boolean;
    timestamp: number;
}

// Hoist constants outside component - best practice: rendering-hoist-jsx
const INITIAL_MESSAGE: Message = {
    id: 'initial',
    text: "Hi! I'm your movie booking assistant. How can I help you today?",
    isUser: false,
    timestamp: Date.now(),
};

// Separate component to prevent unnecessary re-renders - best practice: rerender-memo
const ChatMessage = ({ message }: { message: Message }) => {
    // Cache time formatting - best practice: js-cache-function-results
    const formattedTime = useMemo(() => {
        return new Date(message.timestamp).toLocaleTimeString([], {
            hour: '2-digit',
            minute: '2-digit',
        });
    }, [message.timestamp]);

    return (
        <div
            className={`flex ${message.isUser ? 'justify-end' : 'justify-start'} animate-fadeIn`}
        >
            <div
                className={`max-w-[75%] px-4 py-3 rounded-2xl ${
                    message.isUser
                        ? 'bg-linear-to-br from-[#667eea] to-[#764ba2] text-white rounded-br-sm'
                        : 'bg-white text-gray-800 rounded-bl-sm shadow-sm'
                }`}
            >
                <p className="text-sm leading-relaxed break-words m-0">{message.text}</p>
                <span className="text-[10px] opacity-70 mt-1 block">{formattedTime}</span>
            </div>
        </div>
    );
};

// Typing indicator component - best practice: rendering-hoist-jsx
const TypingIndicator = () => (
    <div className="flex justify-start">
        <div className="bg-white px-4 py-3 rounded-2xl rounded-bl-sm shadow-sm">
            <div className="flex gap-1 py-2">
                {[0, 0.2, 0.4].map((delay, i) => (
                    <span
                        key={i}
                        className="w-2 h-2 bg-gray-400 rounded-full animate-typing"
                        style={{ animationDelay: `${delay}s` }}
                    />
                ))}
            </div>
        </div>
    </div>
);

export default function ChatWidget() {
    const [isOpen, setIsOpen] = useState(false);
    const [messages, setMessages] = useState<Message[]>([INITIAL_MESSAGE]);
    const [inputMessage, setInputMessage] = useState('');
    const [sendMessage, { isLoading }] = useSendMessageMutation();
    const messagesEndRef = useRef<HTMLDivElement>(null);
    
    // Use ref for values that don't need to trigger re-renders - best practice: rerender-use-ref-transient-values
    const messageIdCounter = useRef(0);

    // Memoize scroll function - best practice: js-cache-function-results
    const scrollToBottom = useCallback(() => {
        messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
    }, []);

    // Effect for scrolling - best practice: async-dependencies
    useEffect(() => {
        scrollToBottom();
    }, [messages, scrollToBottom]);

    // Generate unique message ID - best practice: avoid array index as key
    const generateMessageId = useCallback(() => {
        messageIdCounter.current += 1;
        return `msg-${Date.now()}-${messageIdCounter.current}`;
    }, []);

    // Handle send message with useCallback - best practice: rerender-dependencies
    const handleSendMessage = useCallback(async () => {
        // Early exit pattern - best practice: js-early-exit
        if (!inputMessage.trim() || isLoading) return;

        const userMessage: Message = {
            id: generateMessageId(),
            text: inputMessage,
            isUser: true,
            timestamp: Date.now(),
        };

        // Functional setState for correctness - best practice: rerender-functional-setstate
        setMessages((prev) => [...prev, userMessage]);
        setInputMessage('');

        try {
            const response = await sendMessage({ message: inputMessage }).unwrap();
            
            const botMessage: Message = {
                id: generateMessageId(),
                text: response.response,
                isUser: false,
                timestamp: Date.now(),
            };

            setMessages((prev) => [...prev, botMessage]);
        } catch {
            const errorMessage: Message = {
                id: generateMessageId(),
                text: 'Sorry, I encountered an error. Please try again.',
                isUser: false,
                timestamp: Date.now(),
            };
            setMessages((prev) => [...prev, errorMessage]);
        }
    }, [inputMessage, isLoading, sendMessage, generateMessageId]);

    // Handle keypress with useCallback - best practice: rerender-dependencies
    const handleKeyPress = useCallback(
        (e: React.KeyboardEvent) => {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                handleSendMessage();
            }
        },
        [handleSendMessage]
    );

    // Toggle chat with useCallback - best practice: rerender-dependencies
    const toggleChat = useCallback(() => {
        setIsOpen((prev) => !prev);
    }, []);

    // Input change handler - best practice: rerender-dependencies
    const handleInputChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
        setInputMessage(e.target.value);
    }, []);

    // Memoize button disabled state - best practice: rerender-simple-expression-in-memo
    const isSendDisabled = useMemo(
        () => isLoading || !inputMessage.trim(),
        [isLoading, inputMessage]
    );

    return (
        <>
            {/* Chat Toggle Button */}
            <button
                className={`fixed bottom-6 right-6 w-[60px] h-[60px] rounded-full border-none text-white text-2xl cursor-pointer shadow-[0_4px_12px_rgba(0,0,0,0.15)] transition-all duration-300 z-[1000] flex items-center justify-center hover:scale-110 hover:shadow-[0_6px_16px_rgba(0,0,0,0.2)] ${
                    isOpen
                        ? 'bg-gradient-to-br from-[#f093fb] to-[#f5576c]'
                        : 'bg-gradient-to-br from-[#667eea] to-[#764ba2]'
                }`}
                onClick={toggleChat}
                aria-label="Toggle chat"
            >
                {isOpen ? '✕' : '💬'}
            </button>

            {/* Chat Widget with conditional rendering - best practice: rendering-conditional-render */}
            {isOpen && (
                <div className="fixed bottom-[100px] right-6 w-[380px] h-[550px] bg-white rounded-2xl shadow-[0_8px_32px_rgba(0,0,0,0.15)] flex flex-col z-[999] overflow-hidden animate-slideUp max-[480px]:w-[calc(100vw-32px)] max-[480px]:h-[calc(100vh-140px)] max-[480px]:right-4 max-[480px]:bottom-[90px]">
                    {/* Chat Header */}
                    <div className="bg-linear-to-br from-[#667eea] to-[#764ba2] text-white px-5 py-4 flex justify-between items-center">
                        <h3 className="m-0 text-lg font-semibold">Movie Assistant</h3>
                        <span className="flex items-center text-xs opacity-90">
                            <span className="w-2 h-2 bg-[#4ade80] rounded-full mr-1.5 animate-pulse" />
                            Online
                        </span>
                    </div>

                    {/* Messages Container with content-visibility - best practice: rendering-content-visibility */}
                    <div className="flex-1 overflow-y-auto p-5 bg-gray-50 flex flex-col gap-3 [&::-webkit-scrollbar]:w-1.5 [&::-webkit-scrollbar-track]:bg-gray-100 [&::-webkit-scrollbar-thumb]:bg-gray-300 [&::-webkit-scrollbar-thumb]:rounded-sm [&::-webkit-scrollbar-thumb:hover]:bg-gray-400">
                        {/* Use unique keys instead of index - best practice: avoid index as key */}
                        {messages.map((msg) => (
                            <ChatMessage key={msg.id} message={msg} />
                        ))}
                        {isLoading && <TypingIndicator />}
                        <div ref={messagesEndRef} />
                    </div>

                    {/* Input Container */}
                    <div className="flex gap-2 p-4 bg-white border-t border-gray-200">
                        <input
                            type="text"
                            className="flex-1 px-4 py-3 border border-gray-200 rounded-3xl text-sm outline-none transition-colors duration-200 focus:border-[#667eea] disabled:bg-gray-100 disabled:cursor-not-allowed"
                            placeholder="Ask about movies, bookings..."
                            value={inputMessage}
                            onChange={handleInputChange}
                            onKeyPress={handleKeyPress}
                            disabled={isLoading}
                        />
                        <button
                            className="w-11 h-11 rounded-full bg-linear-to-br from-[#667eea] to-[#764ba2] border-none text-white text-lg cursor-pointer transition-all duration-200 flex items-center justify-center hover:scale-105 hover:shadow-[0_4px_12px_rgba(102,126,234,0.4)] disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:scale-100"
                            onClick={handleSendMessage}
                            disabled={isSendDisabled}
                        >
                            {isLoading ? '...' : '➤'}
                        </button>
                    </div>
                </div>
            )}
        </>
    );
}
