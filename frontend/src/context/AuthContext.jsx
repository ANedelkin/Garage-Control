import { createContext, useContext, useState, useEffect } from 'react';
import { authApi } from '../services/authApi';

const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
    const [accesses, setAccesses] = useState([]);
    const [loggedIn, setLoggedIn] = useState(false);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const initAuth = async () => {
            const storedLoggedIn = localStorage.getItem('LoggedIn');
            const storedAccesses = localStorage.getItem('accesses');

            if (storedLoggedIn) {
                setLoggedIn(true);
                if (storedAccesses) {
                    try {
                        setAccesses(JSON.parse(storedAccesses));
                    } catch (e) {
                        console.error("Failed to parse stored accesses", e);
                    }
                }
                setLoading(false);
            } else {
                try {
                    const data = await authApi.refreshToken();
                    if (data && data.success) {
                        setLoggedIn(true);
                        localStorage.setItem('LoggedIn', 'true');
                        if (data.accesses) {
                            setAccesses(data.accesses);
                            localStorage.setItem('accesses', JSON.stringify(data.accesses));
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
        localStorage.removeItem('LoggedIn');
        localStorage.removeItem('accesses');
        localStorage.removeItem('HasWorkshop');
    };

    return (
        <AuthContext.Provider value={{ accesses, loggedIn, loading, login, logout }}>
            {children}
        </AuthContext.Provider>
    );
};

export const useAuth = () => useContext(AuthContext);
