import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useSelector, useDispatch } from 'react-redux';
import type { RootState } from '../store/store';
import { logout } from '../store/authSlice';
import { useLogoutMutation } from '../store/authApi';

export default function Navbar() {
    const { isAuthenticated, user } = useSelector((state: RootState) => state.auth);
    const dispatch = useDispatch();
    const navigate = useNavigate();
    const location = useLocation();
    const [logoutApi] = useLogoutMutation();

    const handleLogout = async () => {
        try { await logoutApi().unwrap(); } catch { /* ignore */ }
        dispatch(logout());
        navigate('/login');
    };

    const isActive = (path: string) => location.pathname === path || location.pathname.startsWith(path + '/');

    return (
        <nav style={{
            position: 'fixed',
            top: 0,
            left: 0,
            right: 0,
            zIndex: 50,
            backdropFilter: 'blur(20px)',
            background: 'rgba(15, 10, 30, 0.85)',
            borderBottom: '1px solid rgba(139, 92, 246, 0.15)',
        }}>
            <div style={{
                maxWidth: 1200,
                margin: '0 auto',
                padding: '0 24px',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'space-between',
                height: 64,
            }}>
                <Link to="/movies" style={{
                    textDecoration: 'none',
                    fontSize: 22,
                    fontWeight: 800,
                    background: 'linear-gradient(135deg, #a78bfa, #ec4899)',
                    WebkitBackgroundClip: 'text',
                    WebkitTextFillColor: 'transparent',
                    letterSpacing: -0.5,
                }}>
                    🎬 CineBook
                </Link>

                <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                    <Link to="/movies" style={{
                        textDecoration: 'none',
                        padding: '8px 16px',
                        borderRadius: 8,
                        fontSize: 14,
                        fontWeight: 500,
                        color: isActive('/movies') ? '#a78bfa' : 'rgba(255,255,255,0.7)',
                        background: isActive('/movies') ? 'rgba(139, 92, 246, 0.1)' : 'transparent',
                        transition: 'all 0.2s',
                    }}>
                        Movies
                    </Link>

                    {isAuthenticated && (
                        <Link to="/my-bookings" style={{
                            textDecoration: 'none',
                            padding: '8px 16px',
                            borderRadius: 8,
                            fontSize: 14,
                            fontWeight: 500,
                            color: isActive('/my-bookings') ? '#a78bfa' : 'rgba(255,255,255,0.7)',
                            background: isActive('/my-bookings') ? 'rgba(139, 92, 246, 0.1)' : 'transparent',
                            transition: 'all 0.2s',
                        }}>
                            My Bookings
                        </Link>
                    )}

                    <div style={{ width: 1, height: 24, background: 'rgba(255,255,255,0.1)', margin: '0 8px' }} />

                    {isAuthenticated ? (
                        <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
                            <span style={{ fontSize: 13, color: 'rgba(255,255,255,0.5)' }}>
                                {user?.fullName}
                            </span>
                            <button onClick={handleLogout} style={{
                                padding: '8px 16px',
                                borderRadius: 8,
                                border: '1px solid rgba(239, 68, 68, 0.3)',
                                background: 'rgba(239, 68, 68, 0.1)',
                                color: '#ef4444',
                                fontSize: 13,
                                fontWeight: 500,
                                cursor: 'pointer',
                                transition: 'all 0.2s',
                            }}>
                                Logout
                            </button>
                        </div>
                    ) : (
                        <Link to="/login" style={{
                            textDecoration: 'none',
                            padding: '8px 20px',
                            borderRadius: 8,
                            background: 'linear-gradient(135deg, #8b5cf6, #6d28d9)',
                            color: '#fff',
                            fontSize: 13,
                            fontWeight: 600,
                        }}>
                            Login
                        </Link>
                    )}
                </div>
            </div>
        </nav>
    );
}
