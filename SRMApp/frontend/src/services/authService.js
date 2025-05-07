// src/services/authService.js
import axios from 'axios';

const API_URL = import.meta.env.VUE_APP_JWT_SERVICE_URL;

class AuthService {
    async login(user) {
        // MVP: Simulate API call and successful login
        console.log('Attempting login for:', user.username);
        if (user.username === 'admin' && user.password === 'admin') {
            const mockToken = 'fake-admin-jwt-token';
            const mockUser = { username: 'admin', role: 'admin' };
            localStorage.setItem('user', JSON.stringify(mockUser));
            localStorage.setItem('token', mockToken);
            return { token: mockToken, user: mockUser };
        } else if (user.username === 'customer' && user.password === 'customer') {
            const mockToken = 'fake-customer-jwt-token';
            const mockUser = { username: 'customer', role: 'customer' };
            localStorage.setItem('user', JSON.stringify(mockUser));
            localStorage.setItem('token', mockToken);
            return { token: mockToken, user: mockUser };
        } else {
            // Simulate API error response
            const error = new Error('Invalid credentials');
            error.response = {
                data: { message: 'Invalid credentials' },
                status: 401
            };
            throw error;
        }

        // REAL IMPLEMENTATION (later)
        // try {
        //     const response = await axios.post(API_URL + '/login', { // Or your actual login endpoint
        //         username: user.username,
        //         password: user.password
        //     });
        //     if (response.data.token) {
        //         // Assuming backend sends user info and token
        //         localStorage.setItem('user', JSON.stringify(response.data.user));
        //         localStorage.setItem('token', response.data.token);
        //     }
        //     return response.data;
        // } catch (error) {
        //     console.error("Login error:", error.response ? error.response.data : error.message);
        //     throw error;
        // }
    }

    logout() {
        localStorage.removeItem('user');
        localStorage.removeItem('token');
    }

    getCurrentUser() {
        const user = localStorage.getItem('user');
        return user ? JSON.parse(user) : null;
    }

    getToken() {
        return localStorage.getItem('token');
    }
}

export default new AuthService();