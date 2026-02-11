import { Link } from 'react-router-dom';
import { useGetMoviesQuery } from '../store/movieApi';

export default function MoviesPage() {
    const { data: movies, isLoading, error } = useGetMoviesQuery();

    return (
        <div style={{ minHeight: '100vh', background: '#0a0618', paddingTop: 96 }}>
            <div style={{ maxWidth: 1200, margin: '0 auto', padding: '0 24px 64px' }}>
                <div style={{ marginBottom: 48 }}>
                    <h1 style={{
                        fontSize: 40,
                        fontWeight: 800,
                        background: 'linear-gradient(135deg, #fff, #a78bfa)',
                        WebkitBackgroundClip: 'text',
                        WebkitTextFillColor: 'transparent',
                        margin: 0,
                    }}>
                        Now Showing
                    </h1>
                    <p style={{ color: 'rgba(255,255,255,0.5)', fontSize: 16, marginTop: 8 }}>
                        Book your tickets for the latest movies
                    </p>
                </div>

                {isLoading && (
                    <div style={{ textAlign: 'center', padding: 80 }}>
                        <div style={{
                            width: 40, height: 40,
                            border: '3px solid rgba(139,92,246,0.2)',
                            borderTopColor: '#8b5cf6',
                            borderRadius: '50%',
                            animation: 'spin 0.8s linear infinite',
                            margin: '0 auto',
                        }} />
                    </div>
                )}

                {error && (
                    <div style={{
                        padding: 24, borderRadius: 12,
                        background: 'rgba(239,68,68,0.1)',
                        border: '1px solid rgba(239,68,68,0.2)',
                        color: '#ef4444', textAlign: 'center',
                    }}>
                        Failed to load movies. Is the backend running?
                    </div>
                )}

                <div style={{
                    display: 'grid',
                    gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))',
                    gap: 24,
                }}>
                    {movies?.map(movie => (
                        <Link key={movie.id} to={`/movies/${movie.id}`} style={{ textDecoration: 'none' }}>
                            <div style={{
                                borderRadius: 16,
                                overflow: 'hidden',
                                background: 'rgba(255,255,255,0.03)',
                                border: '1px solid rgba(255,255,255,0.06)',
                                transition: 'all 0.3s ease',
                                cursor: 'pointer',
                            }}
                                onMouseEnter={e => {
                                    e.currentTarget.style.transform = 'translateY(-6px)';
                                    e.currentTarget.style.borderColor = 'rgba(139,92,246,0.3)';
                                    e.currentTarget.style.boxShadow = '0 20px 40px rgba(139,92,246,0.1)';
                                }}
                                onMouseLeave={e => {
                                    e.currentTarget.style.transform = 'translateY(0)';
                                    e.currentTarget.style.borderColor = 'rgba(255,255,255,0.06)';
                                    e.currentTarget.style.boxShadow = 'none';
                                }}>
                                <div style={{
                                    position: 'relative',
                                    paddingTop: '150%',
                                    background: 'linear-gradient(135deg, #1a1040, #0d0620)',
                                }}>
                                    <img
                                        src={movie.posterUrl}
                                        alt={movie.title}
                                        style={{
                                            position: 'absolute',
                                            top: 0, left: 0,
                                            width: '100%', height: '100%',
                                            objectFit: 'cover',
                                        }}
                                        onError={e => {
                                            (e.target as HTMLImageElement).style.display = 'none';
                                        }}
                                    />
                                    <span style={{
                                        position: 'absolute',
                                        top: 12, right: 12,
                                        padding: '4px 10px',
                                        borderRadius: 6,
                                        background: 'rgba(139,92,246,0.9)',
                                        color: '#fff',
                                        fontSize: 12,
                                        fontWeight: 600,
                                    }}>
                                        {movie.rating}
                                    </span>
                                </div>

                                <div style={{ padding: '16px 20px 20px' }}>
                                    <h3 style={{
                                        margin: 0,
                                        fontSize: 18,
                                        fontWeight: 700,
                                        color: '#fff',
                                        lineHeight: 1.3,
                                    }}>
                                        {movie.title}
                                    </h3>
                                    <div style={{
                                        display: 'flex',
                                        alignItems: 'center',
                                        gap: 12,
                                        marginTop: 10,
                                        fontSize: 13,
                                        color: 'rgba(255,255,255,0.45)',
                                    }}>
                                        <span style={{
                                            padding: '2px 8px',
                                            borderRadius: 4,
                                            background: 'rgba(139,92,246,0.15)',
                                            color: '#a78bfa',
                                            fontSize: 12,
                                        }}>
                                            {movie.genre}
                                        </span>
                                        <span>{movie.durationMinutes} min</span>
                                    </div>
                                </div>
                            </div>
                        </Link>
                    ))}
                </div>
            </div>

            <style>{`
                @keyframes spin {
                    to { transform: rotate(360deg); }
                }
            `}</style>
        </div>
    );
}
