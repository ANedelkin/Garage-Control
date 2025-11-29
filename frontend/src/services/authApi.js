const API_BASE_URL = 'https://localhost:5173/api/auth';

export const authApi = {
    register: async (email, password) => {
        try {
            const response = await fetch(`${API_BASE_URL}/signup`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                credentials: 'include',
                body: JSON.stringify({ email, password }),
            });
            
            const data = await response.json();
            
            console.log(data.message);

            if (!response.ok) {
                throw new Error(data.message || 'Registration failed');
            }

            return data;
        } catch (error) {
            console.error('Registration error:', error);
            throw error;
        }
    },

    login: async (email, password) => {
        try {
            const response = await fetch(`${API_BASE_URL}/login`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                credentials: 'include',
                body: JSON.stringify({ email, password }),
            });

            const data = await response.json();

            if (!response.ok) {
                throw new Error(data.message || 'Login failed');
            }

            return JSON.parse(data);
        } catch (error) {
            console.error('Login error:', error);
            throw error;
        }
    },

    logout: async () => {
        try {
            const response = await fetch(`${API_BASE_URL}/logout`, {
                method: 'POST',
                credentials: 'include',
            });

            if (!response.ok) {
                throw new Error('Logout failed');
            }

            return await response.json();
        } catch (error) {
            console.error('Logout error:', error);
            throw error;
        }
    },

    refreshToken: async () => {
        try {
            const response = await fetch(`${API_BASE_URL}/refresh`, {
                method: 'POST',
                credentials: 'include',
            });

            const data = await response.json();

            if (!response.ok) {
                throw new Error('Token refresh failed');
            }

            return JSON.parse(data);
        } catch (error) {
            console.error('Token refresh error:', error);
            throw error;
        }
    },
};
