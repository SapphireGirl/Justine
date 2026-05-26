import { useEffect, useState } from "react";
import {
  Card,
  CardContent,
  Typography,
  CardActions,
  Button,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Box,
} from "@mui/material";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import Grid from '@mui/material/Grid';

// css
import "./App.css";

interface Basket {
  BasketId: number;
  CustomerName: string;
  Products: number[]; // list of ProductId values
  TotalPrice: number;
  CreatedAt: string;
  UpdatedAt: string;
}

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

// Fetch products from API and return a Promise<Product[]>
async function getProducts(): Promise<Product[]> {
  const resp = await fetch("/api/products", {
    method: "GET",
    headers: { Accept: "application/json" },
    credentials: "include",
  });

  if (!resp.ok) {
    // You can handle specific status codes here (404 => no table, etc.)
    throw new Error(`Failed to load products: ${resp.status} ${resp.statusText}`);
  }

  const payload = await resp.json().catch(() => null);
  if (!Array.isArray(payload)) return [];

  // Normalize shape loosely to Product interface
  return payload.map((p: Product) => ({
    ProductId: Number(p.ProductId ?? 0),
    Name: String(p.Name ?? ""),
    Description: p.Description ?? "",
    Price: Number(p.Price ?? 0.0),
    ImageUrl: p.ImageUrl ?? "",
    Quantity: Number(p.Quantity ?? 0),
    CreatedAt: p.CreatedAt ?? null,
    UpdatedAt: p.UpdatedAt ?? null,
  })) as Product[];
}

export default function Baskets({ initialBaskets }: { initialBaskets?: Basket[] }) {
  // Removed dependency on a custom useBasket hook.
  // Use local state (optionally seeded via initialBaskets) and a simple remove handler.
  const [baskets, setBaskets] = useState<Basket[]>(initialBaskets ?? []);
  const [productMap, setProductMap] = useState<Record<number, Product>>({});

  useEffect(() => {
    let mounted = true;
    getProducts()
      .then((products) => {
        if (!mounted) return;
        const map: Record<number, Product> = {};
        products.forEach((p) => {
          map[p.ProductId] = p;
        });
        setProductMap(map);
      })
      .catch((err) => {
        console.error("Failed to load products for baskets:", err);
        setProductMap({});
      });
    return () => {
      mounted = false;
    };
  }, []);

  // simple remove implementation that updates local state
  const removeFromBasket = (basketId: number | undefined) => {
    if (basketId === undefined) return;
    setBaskets((prev) => prev.filter((b) => b.BasketId !== basketId));
  };

  return (
    <Grid container spacing={4}>
      {baskets.map((basket: Basket) => {
        const productDetails = (basket.Products ?? []).map((id) => productMap[id]).filter(Boolean);
        return (
          <Grid size={{ xs: 12, sm: 6, md: 4 }}>
            <Accordion>
              <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                <Box sx={{ display: "flex", flexDirection: "column", width: "100%" }}>
                  <Typography variant="h6">{basket.CustomerName}</Typography>
                  <Typography variant="body2" color="text.secondary">
                    {productDetails.length} item(s) — Total: ${basket.TotalPrice.toFixed(2)}
                  </Typography>
                </Box>
              </AccordionSummary>
              <AccordionDetails>
                <Card variant="outlined">
                  <CardContent>
                    {productDetails.length === 0 ? (
                      <Typography>No product details available yet.</Typography>
                    ) : (
                      productDetails.map((p) => (
                        <Box key={p.ProductId} sx={{ mb: 2 }}>
                          <Typography variant="subtitle1">{p.Name}</Typography>
                          <Typography variant="body2" color="text.secondary">
                            {p.Description ?? "No description"}
                          </Typography>
                          <Typography variant="body2">Quantity: {p.Quantity}</Typography>
                          <Typography variant="body2">Price: ${p.Price.toFixed(2)}</Typography>
                        </Box>
                      ))
                    )}
                  </CardContent>
                  <CardActions>
                    <Button size="small" color="secondary" onClick={() => removeFromBasket(basket.BasketId)}>
                      Remove
                    </Button>
                  </CardActions>
                </Card>
              </AccordionDetails>
            </Accordion>
          </Grid>
        );
      })}
    </Grid>
  );
}
