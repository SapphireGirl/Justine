import { useEffect, useState } from "react";
import { ToastContainer, toast, Bounce } from 'react-toastify';
import { useNavigate } from "react-router-dom";
import {
    AppBar,
    Toolbar,
    Container,
    Card,
    CardMedia,
    CardContent,
    Typography,
    CardActions,
    Button,
    Accordion,
    AccordionSummary,
    AccordionDetails,
    Box,
} from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import Grid from '@mui/material/Grid';
// css
import '../App.css';

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

        const fetchProducts = async () => {
            setLoading(true);
            setError(null);

            const rawApiBase = (import.meta as unknown as { env?: { VITE_API_BASE?: string } }).env?.VITE_API_BASE;
            const apiBase = typeof rawApiBase === "string" && rawApiBase.length > 0
                ? rawApiBase.replace(/\/$/, "")
                : window.location.origin.replace(/\/$/, "");
            const productsUrl = `${apiBase}/api/products`;

            try {
                const resp = await fetch(productsUrl, {
                    method: "GET",
                    headers: { Accept: "application/json" },
                    credentials: "include"
                });

                if (resp.status === 401 || resp.status === 403) {
                    setProducts([]);
                    setError("Not authenticated. Please sign in.");
                    return;
                }

                if (resp.status === 204) {
                    setProducts([]);
                    return;
                }

                if (resp.status === 400 || resp.status === 404) {
                    // treat as first-run: table missing or empty
                    setProducts([]);
                    setError(null);
                    toast.info("Products table missing or empty. Use Admin to create/seed data.", { position: "top-center", transition: Bounce });
                    return;
                }

                if (!resp.ok) {
                    const text = await resp.text().catch(() => "");
                    setProducts([]);
                    setError(`Server returned ${resp.status}: ${text || resp.statusText}`);
                    return;
                }

                const payload: unknown = await resp.json().catch(() => null);

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
            } finally {
                setLoading(false);
            }
        };

        fetchProducts();
        setUserName("justine");
    }, []);

    const isAdmin = userName?.toLowerCase() === "justine";
    const shouldShowAdmin = isAdmin && !loading && !error && products.length === 0;

    return (
        <Box sx={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>
            <ToastContainer position="top-center" autoClose={3000} transition={Bounce} />

            {/* Header */}
            <AppBar position="static">
                <Toolbar>
                    <Typography variant="h6" sx={{ flexGrow: 1 }}>
                        Products
                    </Typography>
                    {shouldShowAdmin && (
                        <Button color="inherit" onClick={() => navigate("/admin")}>Admin</Button>
                    )}
                </Toolbar>
            </AppBar>

            {/* Main content */}
            <Container component="main" sx={{ py: 4, flexGrow: 1 }}>
                {loading && <Typography>Loading products...</Typography>}
                {error && (
                    <Box role="alert" sx={{ mb: 2 }}>
                        <Typography color="error">{error}</Typography>
                    </Box>
                )}

                <Grid container spacing={3}>
                    {products.length === 0 && !loading && !error && (
                        <Grid key="no-products" size={{ xs: 12, sm: 6, md: 4 }}>
                            <Typography>No products available.</Typography>
                        </Grid>
                    )}

                    {products.map((product) => (
                        <Grid key={product.ProductId ?? `${product.Name}-${Math.random()}`} size={{ xs: 12, sm: 6, md: 4 }}>
                            <Accordion>
                                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                    <Box sx={{ display: 'flex', justifyContent: 'space-between', width: '100%', alignItems: 'center' }}>
                                        <Typography variant="subtitle1">{product.Name}</Typography>
                                        <Typography variant="subtitle2">${product.Price.toFixed(2)}</Typography>
                                    </Box>
                                </AccordionSummary>

                                <AccordionDetails>
                                    <Card variant="outlined" sx={{ display: 'flex', flexDirection: 'column' }}>
                                        {product.ImageUrl ? (
                                            <CardMedia
                                                component="img"
                                                image={product.ImageUrl}
                                                alt={product.Name}
                                                sx={{ height: 180, objectFit: 'contain' }}
                                            />
                                        ) : null}
                                        <CardContent>
                                            <Typography variant="h6">{product.Name}</Typography>
                                            <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                                                {product.Description || "No description"}
                                            </Typography>
                                            <Typography variant="body2">Quantity: {product.Quantity}</Typography>
                                            <Typography variant="body2">Price: ${product.Price.toFixed(2)}</Typography>
                                            {product.CreatedAt && <Typography variant="caption" display="block">Created: {product.CreatedAt}</Typography>}
                                            {product.UpdatedAt && <Typography variant="caption" display="block">Updated: {product.UpdatedAt}</Typography>}
                                        </CardContent>
                                        <CardActions>
                                            <Button size="small" onClick={() => navigate(`/products/${product.ProductId}`)}>View</Button>
                                            <Button size="small" onClick={() => toast.info("Add to basket not implemented")}>Add to Basket</Button>
                                        </CardActions>
                                    </Card>
                                </AccordionDetails>
                            </Accordion>
                        </Grid>
                    ))}
                </Grid>
            </Container>

            {/* Footer */}
            <Box component="footer" sx={{ py: 2, textAlign: 'center', bgcolor: 'background.paper' }}>
                <Container maxWidth="md">
                    <Typography variant="body2" color="text.secondary">
                        © {new Date().getFullYear()} Justine Store — Demo
                    </Typography>
                </Container>
            </Box>
        </Box>
    );
};

export default ProductList;
