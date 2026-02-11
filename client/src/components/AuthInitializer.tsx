import { useEffect, useState } from 'react';
import { useDispatch } from 'react-redux';
import { useGetMeQuery } from '../store/authApi';
import { setCredentials } from '../store/authSlice';

interface AuthInitializerProps {
    children: React.ReactNode;
}

export default function AuthInitializer({ children }: AuthInitializerProps) {
    const dispatch = useDispatch();
    const { data: user, isLoading, isSuccess } = useGetMeQuery();
    const [isInitialized, setIsInitialized] = useState(false);

    useEffect(() => {
        if (user) {
            dispatch(setCredentials({ user }));
        }

        if (!isLoading) {
            setIsInitialized(true);
        }
    }, [user, isLoading, dispatch, isSuccess]);

    if (!isInitialized) {
        return (
            <div className="min-h-screen flex items-center justify-center bg-gray-900 text-white">
                <div className="flex flex-col items-center gap-4">
                    <div className="animate-spin h-10 w-10 border-4 border-primary border-t-transparent rounded-full"></div>
                    <p className="text-gray-400">Loading session...</p>
                </div>
            </div>
        );
    }

    return <>{children}</>;
}
