import { useEffect, useState } from "react";
import { ToastContainer, toast, Bounce } from 'react-toastify';
import { useNavigate } from "react-router-dom";

interface Product {
    ProductId: number;
    Name: string;
    Description?: string | null;
    Price: number;
    ImageUrl?: string | null;
    Quantity: number;
    CreatedAt?: string | null;
    UpdatedAt?: string | null;
}

const isObject = (v: unknown): v is Record<string, unknown> => typeof v === "object" && v !== null;

const hasProductIdentifier = (o: Record<string, unknown>) => {
    return (
        typeof o["ProductId"] === "string" ||
        typeof o["ProductId"] === "number" ||
        typeof o["productId"] === "string" ||
        typeof o["productId"] === "number" ||
        typeof o["id"] === "string" ||
        typeof o["id"] === "number" ||
        typeof o["name"] === "string" ||
        typeof o["Name"] === "string"
    );
};

const toString = (v: unknown): string => {
    if (typeof v === "string") return v;
    if (typeof v === "number") return String(v);
    if (v instanceof Date) return v.toISOString();
    return "";
};

const toNumber = (v: unknown): number => {
    if (typeof v === "number") return v;
    if (typeof v === "string") {
        const n = Number(v);
        return Number.isFinite(n) ? n : 0;
    }
    if (v instanceof Date) return Math.floor(v.getTime());
    return 0;
};

const ProductList = () => {
    const [products, setProducts] = useState<Product[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [userName, setUserName] = useState<string>("");

    const navigate = useNavigate();

    useEffect(() => {
        toast.info('🦄 Fetching products...', {
            position: "top-center",
            autoClose: 3000,
            transition: Bounce,
        });

        // if User is Justine, show admin button. This is a placeholder for real auth logic.
        // Justine should be able to see the admin button and access the admin page, while others should not.

        const fetchProducts = async () => {
            setLoading(true);
            setError(null);

            // Read Vite env safely and use a robust fallback when it's not set.
            // Prefer VITE_API_BASE if provided; otherwise fall back to the current origin
            // so requests target the same host that served the app (avoids empty base producing wrong URL).
            const rawApiBase = (import.meta as unknown as { env?: { VITE_API_BASE?: string } }).env?.VITE_API_BASE;
            const apiBase = typeof rawApiBase === "string" && rawApiBase.length > 0
                ? rawApiBase.replace(/\/$/, "")
                : window.location.origin.replace(/\/$/, "");
            const url = `${apiBase}/api/products`;

            try {
                const resp = await fetch(url, {
                    method: "GET",
                    headers: { Accept: "application/json" },
                    // Use cookie-based auth (HttpOnly cookies) rather than storing tokens in storage
                    credentials: "include"
                });

                // explicit handling for auth/status codes we care about
                if (resp.status === 401 || resp.status === 403) {
                    setProducts([]);
                    setError("Not authenticated. Please sign in.");
                    return;
                }

                if (resp.status === 204) {
                    setProducts([]);
                    return;
                }

                // Treat 400/404 from server as "table missing / no products" first-run UX
                if (resp.status === 400 || resp.status === 404) {
                    setProducts([]);
                    setError(null);
                    toast.info("Products table missing or empty. Use Admin to create/seed data.", { position: "top-center", transition: Bounce });
                    return;
                }

                if (!resp.ok) {
                    const text = await resp.text().catch(() => "");
                    setProducts([]);
                    setError(`Server returned ${resp.status}: ${text || resp.statusText}`);
                    //toast.error("Failed to fetch products.", { position: "top-center", transition: Bounce });
                    return;
                }

                // parse as unknown and validate without `any`
                const payload: unknown = await resp.json().catch(() => null);

                // Normalize payload into Product[] using the Product interface
                let candidates: Record<string, unknown>[] = [];
                if (Array.isArray(payload)) {
                    candidates = payload.filter(isObject);
                } else if (isObject(payload)) {
                    candidates = [payload];
                } else {
                    candidates = [];
                }

                const normalized: Product[] = candidates
                    .filter(hasProductIdentifier)
                    .map((it) => {
                        // --- CHANGED: produce fields that match Product.cs / Product interface ---
                        const id = toNumber(it["ProductId"] ?? it["productId"] ?? it["id"] ?? 0);
                        const name = toString(it["Name"] ?? it["name"] ?? "");
                        const description = toString(it["Description"] ?? it["description"] ?? "") || null;
                        const price = Number(toNumber(it["Price"] ?? it["price"] ?? 0));
                        const quantity = Number(toNumber(it["Quantity"] ?? it["quantity"] ?? 0));
                        const imageUrl = toString(it["ImageUrl"] ?? it["imageUrl"] ?? null) || null;
                        const createdAt = toString(it["CreatedAt"] ?? it["createdAt"] ?? null) || null;
                        const updatedAt = toString(it["UpdatedAt"] ?? it["updatedAt"] ?? null) || null;

                        return {
                            ProductId: id,
                            Name: name,
                            Description: description,
                            Price: price,
                            ImageUrl: imageUrl,
                            Quantity: quantity,
                            CreatedAt: createdAt,
                            UpdatedAt: updatedAt,
                        } as Product;
                    });

                setProducts(normalized);
            } catch (err) {
                console.error("Fetch products error:", err);
                setProducts([]);
                setError("Failed to fetch products. See console for details.");
                //toast.error("Failed to fetch products.", { position: "top-center", transition: Bounce });
            } finally {
                setLoading(false);
            }
        };

        fetchProducts();
        setUserName("justine");
    }, []);

    // case-insensitive admin check
    const isAdmin = userName?.toLowerCase() === "justine";

    // show admin button only when user is admin, loading finished, no server error, and no products
    const shouldShowAdmin = isAdmin && !loading && !error && products.length === 0;

    return (
        <div>
            <ToastContainer position="top-center" autoClose={3000} transition={Bounce} />
            <h1>Product List</h1>

            {/* Show loading / error inline but keep page visible */}
            {loading && <p>Loading products...</p>}
            {error && (
                <div role="alert" style={{ color: "darkred", marginBottom: 12 }}>
                    <strong>{error}</strong>
                </div>
            )}

            {/* Admin button only visible when there are no products and no server error */}
            {shouldShowAdmin && (
                <div style={{ marginBottom: 12 }}>
                    <div style={{ marginTop: 6 }}>
                        <small>If the table is missing or empty, use Admin to create/seed data.</small>
                    </div>
                    <button
                        aria-label="Go to Admin"
                        onClick={() => navigate("/admin")}
                    >
                        Admin
                    </button>

                </div>
            )}

            {(!loading && products.length === 0 && !error) && <p>No products available.</p>}

            {products.length > 0 && (
                <ul>
                    {products.map((product) => (
                        <li key={product.ProductId}>
                            <h2>{product.Name}</h2>
                            <p>{product.Description}</p>
                            <p>Price: ${product.Price}</p>
                            <p>Quantity: {product.Quantity}</p>
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
};

export default ProductList;
