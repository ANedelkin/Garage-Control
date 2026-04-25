import { createContext, useContext, useState, useEffect } from 'react';
import { authApi } from '../services/authApi';

const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
    const [accesses, setAccesses] = useState([]);
    const [user, setUser] = useState(null);
    const [loggedIn, setLoggedIn] = useState(false);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const initAuth = async () => {
            const storedLoggedIn = localStorage.getItem('LoggedIn');
            const storedAccesses = localStorage.getItem('accesses');
            const storedUser = localStorage.getItem('user');

            if (storedLoggedIn) {
                setLoggedIn(true);
                if (storedAccesses) {
                    try {
                        setAccesses(JSON.parse(storedAccesses));
                    } catch (e) {
                        console.error("Failed to parse stored accesses", e);
                    }
                }
                if (storedUser) {
                    try {
                        setUser(JSON.parse(storedUser));
                    } catch (e) {
                        console.error("Failed to parse stored user", e);
                    }
                }
                setLoading(false);
            } else {
                try {
                    const data = await authApi.refreshToken();
                    if (data) {
                        setLoggedIn(true);
                        localStorage.setItem('LoggedIn', 'true');
                        if (data.accesses) {
                            setAccesses(data.accesses);
                            localStorage.setItem('accesses', JSON.stringify(data.accesses));
                        }
                        if (data.userId || data.workerId) {
                            const userData = { id: data.userId, workerId: data.workerId, userName: data.userName };
                            setUser(userData);
                            localStorage.setItem('user', JSON.stringify(userData));
                        }
                    }
                } catch (e) {
                    // Not logged in or refresh failed
                } finally {
                    setLoading(false);
                }
            }
        };

        initAuth();
    }, []);

    const login = (data) => {
        setLoggedIn(true);
        if (data.accesses) {
            setAccesses(data.accesses);
            localStorage.setItem('accesses', JSON.stringify(data.accesses));
        } else {
            setAccesses([]);
            localStorage.removeItem('accesses');
        }
        if (data.userId || data.workerId) {
            const userData = { id: data.userId, workerId: data.workerId, userName: data.userName };
            setUser(userData);
            localStorage.setItem('user', JSON.stringify(userData));
        } else {
            setUser(null);
            localStorage.removeItem('user');
        }
        localStorage.setItem('LoggedIn', 'true');
        if (data.hasWorkshop !== undefined) {
            localStorage.setItem('HasWorkshop', data.hasWorkshop);
        }
    };

    const logout = async () => {
        try {
            await authApi.logout();
        } catch (e) {
            console.error("Logout failed", e);
        }
        setLoggedIn(false);
        setAccesses([]);
        setUser(null);
        localStorage.removeItem('LoggedIn');
        localStorage.removeItem('accesses');
        localStorage.removeItem('user');
        localStorage.removeItem('HasWorkshop');
    };

    const refreshAuth = async () => {
        try {
            const data = await authApi.refreshToken();
            if (data) {
                if (data.accesses) {
                    setAccesses(data.accesses);
                    localStorage.setItem('accesses', JSON.stringify(data.accesses));
                }
                if (data.userId || data.workerId) {
                    const userData = { id: data.userId, workerId: data.workerId, userName: data.userName };
                    setUser(userData);
                    localStorage.setItem('user', JSON.stringify(userData));
                }
                return data;
            }
        } catch (e) {
            console.error("Manual auth refresh failed", e);
        }
    };

    useEffect(() => {
        const handle403 = () => refreshAuth();
        window.addEventListener('api-403', handle403);
        return () => window.removeEventListener('api-403', handle403);
    }, []);

    return (
        <AuthContext.Provider value={{ accesses, user, loggedIn, loading, login, logout, refreshAuth }}>
            {children}
        </AuthContext.Provider>
    );
};

export const useAuth = () => useContext(AuthContext);
