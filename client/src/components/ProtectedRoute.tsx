import { useSelector } from 'react-redux';
import { Navigate, useLocation } from 'react-router-dom';
import type { RootState } from '../store/store';

interface ProtectedRouteProps {
    children: React.ReactNode;
}

export default function ProtectedRoute({ children }: ProtectedRouteProps) {
    const isAuthenticated = useSelector(
        (state: RootState) => state.auth.isAuthenticated
    );

    const location = useLocation();

    if (!isAuthenticated) {
        console.log("Not authenticated, redirecting to login")
        return <Navigate to="/login" state={{ from: location }} replace />;
    }

    return <>{children}</>;
}
