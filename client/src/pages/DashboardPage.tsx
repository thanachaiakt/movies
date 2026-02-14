import { useSelector, useDispatch } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import type { RootState } from '../store/store';
import { logout } from '../store/authSlice';
import { useLogoutMutation } from '../store/authApi';

export default function DashboardPage() {
    const { user } = useSelector((state: RootState) => state.auth);
    const dispatch = useDispatch();
    const navigate = useNavigate();
    const [logoutApi, { isLoading }] = useLogoutMutation();
    
    async function handleLogout() {
        try {
            await logoutApi().unwrap();
        } catch {
            // Even if the API call fails, we still want to clear local state
        } finally {
            dispatch(logout());
            navigate('/login');
        }
    }

    return (
        <div className="min-h-screen relative overflow-hidden">
            {/* Background effects */}
            <div className="absolute inset-0 -z-10">
                <div className="absolute top-0 left-1/2 -translate-x-1/2 w-[800px] h-[400px] bg-primary/10 rounded-full blur-[150px]" />
                <div className="absolute bottom-0 right-0 w-[500px] h-[300px] bg-accent/8 rounded-full blur-[120px]" />
            </div>

            {/* Nav */}
            <nav className="border-b border-white/5 backdrop-blur-lg bg-surface/30">
                <div className="max-w-6xl mx-auto px-6 py-4 flex items-center justify-between">
                    <div className="flex items-center gap-3">
                        <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-primary to-accent flex items-center justify-center shadow-lg shadow-primary/20">
                            <svg className="w-5 h-5 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                                <path strokeLinecap="round" strokeLinejoin="round" d="M3.375 19.5h17.25m-17.25 0a1.125 1.125 0 01-1.125-1.125M3.375 19.5h1.5C5.496 19.5 6 18.996 6 18.375m-3.75 0V5.625m0 12.75v-1.5c0-.621.504-1.125 1.125-1.125m18.375 2.625V5.625m0 12.75c0 .621-.504 1.125-1.125 1.125m1.125-1.125v-1.5c0-.621-.504-1.125-1.125-1.125m0 3.75h-1.5A1.125 1.125 0 0118 18.375M20.625 4.5H3.375m17.25 0c.621 0 1.125.504 1.125 1.125M20.625 4.5h-1.5C18.504 4.5 18 5.004 18 5.625m3.75 0v1.5c0 .621-.504 1.125-1.125 1.125M3.375 4.5c-.621 0-1.125.504-1.125 1.125M3.375 4.5h1.5C5.496 4.5 6 5.004 6 5.625m-3.75 0v1.5c0 .621.504 1.125 1.125 1.125m0 0h1.5m-1.5 0c-.621 0-1.125.504-1.125 1.125v1.5c0 .621.504 1.125 1.125 1.125m1.5-3.75C5.496 8.25 6 7.746 6 7.125v-1.5M4.875 8.25C5.496 8.25 6 8.754 6 9.375v1.5m0-5.25v5.25m0-5.25C6 5.004 6.504 4.5 7.125 4.5h9.75c.621 0 1.125.504 1.125 1.125m1.125 2.625h1.5m-1.5 0A1.125 1.125 0 0118 7.125v-1.5m1.125 2.625c-.621 0-1.125.504-1.125 1.125v1.5m2.625-2.625c.621 0 1.125.504 1.125 1.125v1.5c0 .621-.504 1.125-1.125 1.125M18 5.625v5.25M7.125 12h9.75m-9.75 0A1.125 1.125 0 016 10.875M7.125 12C6.504 12 6 12.504 6 13.125m0-2.25C6 11.496 5.496 12 4.875 12M18 10.875c0 .621-.504 1.125-1.125 1.125M18 10.875c0 .621.504 1.125 1.125 1.125m-2.25 0c.621 0 1.125.504 1.125 1.125m-12 5.25v-5.25m0 5.25c0 .621.504 1.125 1.125 1.125h9.75c.621 0 1.125-.504 1.125-1.125m-12 0v-1.5c0-.621-.504-1.125-1.125-1.125M18 18.375v-5.25m0 5.25v-1.5c0-.621.504-1.125 1.125-1.125M18 13.125v1.5c0 .621.504 1.125 1.125 1.125M18 13.125c0-.621.504-1.125 1.125-1.125M6 13.125v1.5c0 .621-.504 1.125-1.125 1.125M6 13.125C6 12.504 5.496 12 4.875 12m-1.5 0h1.5m-1.5 0c-.621 0-1.125-.504-1.125-1.125v-1.5c0-.621.504-1.125 1.125-1.125m1.5 3.75c-.621 0-1.125-.504-1.125-1.125v-1.5c0-.621.504-1.125 1.125-1.125" />
                            </svg>
                        </div>
                        <span className="text-lg font-bold bg-gradient-to-r from-text to-text-muted bg-clip-text text-transparent">
                            Movies App
                        </span>
                    </div>

                    <div className="flex items-center gap-4">
                        <div className="hidden sm:flex items-center gap-2 text-sm text-text-muted">
                            <div className="w-8 h-8 rounded-full bg-gradient-to-br from-primary/50 to-accent/50 flex items-center justify-center text-white text-xs font-bold">
                                {user?.fullName?.charAt(0)?.toUpperCase() || '?'}
                            </div>
                            <span>{user?.fullName || 'User'}</span>
                        </div>
                        <button
                            onClick={handleLogout}
                            disabled={isLoading}
                            className="px-4 py-2 text-sm font-medium text-text-muted bg-surface-light/50 border border-white/10 rounded-xl hover:bg-error/10 hover:text-error hover:border-error/30 transition-all duration-200 disabled:opacity-50"
                        >
                            {isLoading ? 'Logging out...' : 'Logout'}
                        </button>
                    </div>
                </div>
            </nav>

            {/* Main content */}
            <main className="max-w-6xl mx-auto px-6 py-12">
                <div className="mb-8">
                    <h1 className="text-4xl font-bold mb-2">
                        Hello, <span className="bg-gradient-to-r from-primary-light to-accent bg-clip-text text-transparent">{user?.fullName || 'User'}</span> 👋
                    </h1>
                    <p className="text-text-muted text-lg">Welcome to your dashboard</p>
                </div>

                {/* Stats cards */}
                <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
                    {[
                        { label: 'Account Status', value: 'Active', icon: '✅', color: 'from-success/20 to-success/5' },
                        { label: 'Email', value: user?.email || '-', icon: '📧', color: 'from-primary/20 to-primary/5' },
                        { label: 'Member Since', value: 'Today', icon: '📅', color: 'from-accent/20 to-accent/5' },
                    ].map((stat) => (
                        <div
                            key={stat.label}
                            className={`backdrop-blur-xl bg-gradient-to-br ${stat.color} border border-white/10 rounded-2xl p-6 hover:border-white/20 transition-all duration-300 hover:scale-[1.02]`}
                        >
                            <div className="text-2xl mb-3">{stat.icon}</div>
                            <p className="text-text-muted text-sm mb-1">{stat.label}</p>
                            <p className="text-text font-semibold truncate">{stat.value}</p>
                        </div>
                    ))}
                </div>

                {/* Auth info card */}
                <div className="backdrop-blur-xl bg-surface/60 border border-white/10 rounded-2xl p-6">
                    <h2 className="text-xl font-bold mb-4 flex items-center gap-2">
                        <span className="w-2 h-2 rounded-full bg-success animate-pulse" />
                        Authentication Info
                    </h2>
                    <div className="space-y-3 text-sm">
                        <div className="flex items-center justify-between py-2 border-b border-white/5">
                            <span className="text-text-muted">JWT Token</span>
                            <span className="text-success font-mono text-xs px-3 py-1 bg-success/10 rounded-lg">Active</span>
                        </div>
                        <div className="flex items-center justify-between py-2 border-b border-white/5">
                            <span className="text-text-muted">Refresh Token</span>
                            <span className="text-success font-mono text-xs px-3 py-1 bg-success/10 rounded-lg">Available</span>
                        </div>
                        <div className="flex items-center justify-between py-2">
                            <span className="text-text-muted">Protected Route</span>
                            <span className="text-primary-light font-mono text-xs px-3 py-1 bg-primary/10 rounded-lg">Authorized</span>
                        </div>
                    </div>
                </div>
            </main>
        </div>
    );
}
