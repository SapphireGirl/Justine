import * as React from "react";
import { Card, CardMedia, CardContent, Typography, CardActions, Button } from "@mui/material";
import { toast } from "react-toastify";

export interface Product {
  ProductId: number;
  Name: string;
  Description?: string | null;
  Price: number;
  ImageUrl?: string | null;
  Quantity: number;
  CreatedAt?: string | null;
  UpdatedAt?: string | null;
}

interface Props {
  product: Product;
  onView?: (id: number) => void;
  onAdd?: (id: number) => void;
}

/**
 * Pure presentational ProductCard. Memoized to avoid unnecessary re-renders.
 * - product: product to render
 * - onView: callback when "View" clicked
 * - onAdd: callback when "Add to Basket" clicked
 */
const ProductCard: React.FC<Props> = ({ product, onView, onAdd }) => {
  const handleView = React.useCallback(() => {
    onView?.(product.ProductId);
  }, [onView, product.ProductId]);

  const handleAdd = React.useCallback(() => {
    if (onAdd) {
      onAdd(product.ProductId);
    } else {
      // lightweight fallback so UI still responds in demos
      toast.info(`Add ${product.Name} to basket (not implemented)`);
    }
  }, [onAdd, product.ProductId, product.Name]);

  return (
    <Card variant="outlined" sx={{ display: "flex", flexDirection: "column", height: "100%" }}>
      {product.ImageUrl ? (
        <CardMedia
          component="img"
          image={product.ImageUrl}
          alt={product.Name}
          sx={{ height: 180, objectFit: "contain" }}
        />
      ) : null}
      <CardContent sx={{ flexGrow: 1 }}>
        <Typography variant="h6">{product.Name}</Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
          {product.Description ?? "No description"}
        </Typography>
        <Typography variant="body2">Quantity: {product.Quantity}</Typography>
        <Typography variant="body2">Price: ${product.Price.toFixed(2)}</Typography>
      </CardContent>

      <CardActions>
        <Button size="small" onClick={handleView}>
          View
        </Button>
        <Button size="small" onClick={handleAdd}>
          Add to Basket
        </Button>
      </CardActions>
    </Card>
  );
};

// Memo comparator: re-render only when identifying fields or callbacks change
function propsAreEqual(prev: Props, next: Props) {
  const p1 = prev.product;
  const p2 = next.product;
  const sameProduct =
    p1.ProductId === p2.ProductId &&
    p1.UpdatedAt === p2.UpdatedAt &&
    p1.Name === p2.Name &&
    p1.Price === p2.Price;
  return sameProduct && prev.onView === next.onView && prev.onAdd === next.onAdd;
}

export default React.memo(ProductCard, propsAreEqual);