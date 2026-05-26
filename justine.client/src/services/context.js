import React from 'react';

const ThemeContext = React.createContext();

const theme = {
    colors: {
        primary: '#333',
        secondary: '#666',
    },
    fontSizes: {
        small: 12,
        medium: 14,
        large: 16,
    },
};

const ThemeProvider = ({ children }) => {
    return (
        <ThemeContext.Provider value={theme}>
            {children}
        </ThemeContext.Provider>
    );
};

export { ThemeProvider, ThemeContext };