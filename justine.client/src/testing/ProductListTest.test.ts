import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import ProductList from '../components/ProductList';
import { MemoryRouter } from 'react-router-dom';

import { describe, it, vi, beforeEach, afterEach, expect } from 'vitest';

const sampleProducts = [
    {
        ProductId: 1,
        Name: 'Test Product',
        Description: 'This is a test product description.',
        Price: 9.99,
        ImageUrl: null,
        Quantity: 5,
        CreatedAt: new Date().toISOString(),
        UpdatedAt: new Date().toISOString(),
    },
];

describe('ProductList', () => {
    const originalFetch = globalThis.fetch;

    beforeEach(() => {
        // Mock fetch to return the sample products
        globalThis.fetch = vi.fn(() =>
            Promise.resolve({
                ok: true,
                status: 200,
                json: async () => sampleProducts,
            } as Response)
        ) as unknown as typeof fetch;
    });

    afterEach(() => {
        vi.resetAllMocks();
        globalThis.fetch = originalFetch;
    });

    it('renders products and shows details when an item is expanded', async () => {
        // Use React.createElement instead of JSX so the .ts file compiles without JSX support
        render(
            React.createElement(
                MemoryRouter,
                { initialEntries: ['/'] },
                React.createElement(ProductList)
            )
        );

        // wait for product name to appear
        const nameNode = await screen.findByText('Test Product');
        expect(nameNode).toBeInTheDocument();

        // expand the accordion by clicking the summary (product name)
        const user = userEvent.setup();
        await user.click(nameNode);

        // product description should become visible
        await waitFor(() => {
            expect(screen.getByText('This is a test product description.')).toBeVisible();
        });

        // show price and quantity too
        expect(screen.getByText('$9.99')).toBeInTheDocument();
        expect(screen.getByText(/Quantity: 5/i)).toBeInTheDocument();
    });
});
