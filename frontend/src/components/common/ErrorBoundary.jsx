import React from 'react';
import ErrorPage from './ErrorPage';

class ErrorBoundary extends React.Component {
    constructor(props) {
        super(props);
        this.state = { hasError: false, error: null };
    }

    static getDerivedStateFromError(error) {
        // Update state so the next render will show the fallback UI.
        return { hasError: true, error };
    }

    componentDidCatch(error, errorInfo) {
        // You can also log the error to an error reporting service
        console.error("Uncaught error:", error, errorInfo);
    }

    render() {
        if (this.state.hasError) {
            // You can render any custom fallback UI
            return (
                <div className="app-container">
                    <ErrorPage 
                        title="Critical Error" 
                        message="The application crashed due to an unexpected error. Please try refreshing the page."
                        details={this.state.error?.toString()}
                    />
                </div>
            );
        }

        return this.props.children;
    }
}

export default ErrorBoundary;
