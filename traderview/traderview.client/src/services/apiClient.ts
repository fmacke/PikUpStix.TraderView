/* eslint-disable @typescript-eslint/no-explicit-any */
import axios from 'axios';
import type { AxiosRequestConfig } from 'axios';

export const API_BASE_URL = import.meta.env.VITE_API_URL || '/api';

const apiClient = axios.create({
    baseURL: API_BASE_URL,
    headers: {
        'Content-Type': 'application/json',
    },
    // default timeout so client doesn't hang indefinitely when backend is unreachable
    timeout: 10000,
    validateStatus: (status) => status >= 200 && status < 300,
});

// Response interceptor for logging
apiClient.interceptors.response.use(
    (response) => {
        console.log('API Response interceptor - Success:', {
            status: response.status,
            statusText: response.statusText,
            url: response.config.url,
            data: response.data,
        });
        return response;
    },
    (error) => {
        console.error('API Response interceptor - Error:', {
            message: error.message,
            response: error.response,
            status: error.response?.status,
            data: error.response?.data,
        });
        return Promise.reject(error);
    }
);

export const api = {
    get: async <T = unknown>(url: string, config?: AxiosRequestConfig) => {
        const response = await apiClient.get<T>(url, config);
        return response.data as T;
    },
    post: async <T = unknown>(url: string, data?: unknown, config?: AxiosRequestConfig) => {
        const response = await apiClient.post<T>(url, data, config);
        return response.data as T;
    },
    put: async <T = unknown>(url: string, data?: unknown, config?: AxiosRequestConfig) => {
        const response = await apiClient.put<T>(url, data, config);
        return response.data as T;
    },
    delete: async <T = unknown>(url: string, config?: AxiosRequestConfig) => {
        const response = await apiClient.delete<T>(url, config);
        return response.data as T;
    },
    // convenience for blob responses
    getBlob: async (url: string, config?: AxiosRequestConfig) => {
        const response = await apiClient.get(url, { ...(config || {}), responseType: 'blob' as const });
        return response.data as Blob;
    },
    client: apiClient,
};

export default api;
