import { useEffect } from 'react';

const usePageTitle = (title) => {
    useEffect(() => {
        const prevTitle = document.title;
        document.title = `${title} - Garage Control`;
        return () => {
            document.title = prevTitle;
        };
    }, [title]);
};

export default usePageTitle;
