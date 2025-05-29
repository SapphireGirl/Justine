import { useEffect, useState } from "react";
import AWS from "aws-sdk";
import { ToastContainer, toast } from 'react-toastify';

const ProductList = () => {
    const [products, setProducts] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        toast.info('🦄 Fetching products from AWS Lambda...', {
            position: "top-center",
            autoClose: 5000,
            hideProgressBar: false,
            closeOnClick: false,
            pauseOnHover: true,
            draggable: true,
            progress: undefined,
            theme: "light",
            transition: Bounce,
        });
        //toast("Fetching products from AWS Lambda...");
        // Initialize AWS SDK and fetch products from Lambda
        const fetchProducts = async () => {
            setLoading(true);
            setError(null);

            // Configure AWS SDK
            AWS.config.update({
                region: "us-east-2", // Replace with your Lambda's region
                credentials: new AWS.CognitoIdentityCredentials({
                    IdentityPoolId: "us-east-1:xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx", // Replace with your Identity Pool ID
                }),
            });

            const lambda = new AWS.Lambda();

            try {
                const params = {
                    FunctionName: "GetAllProductsAsync", 
                    InvocationType: "RequestResponse",
                    Payload: JSON.stringify({}), // Add any payload if required
                };

                const response = await lambda.invoke(params).promise();
                const data = JSON.parse(response.Payload);

                setProducts(data); // Ensure your Lambda returns the product list in the expected format

            } catch (e) {
                setError("Failed to fetch all products.");
            } finally {
                setLoading(false);
            }
        };

        fetchProducts();
    }, []);

    if (loading) return <p>Loading...</p>;
    if (error) return <p>{error}</p>;

    return (
        <div>
            <ToastContainer
                position="top-center"
                autoClose={5000}
                hideProgressBar={false}
                newestOnTop={false}
                closeOnClick={false}
                rtl={false}
                pauseOnFocusLoss
                draggable
                pauseOnHover
                theme="light"
                transition={Bounce}
            />
            <h1>Product List</h1>
            <ul>
                {products.map((product) => (
                    <li key={product.id}>
                        <h2>{product.name}</h2>
                        <p>{product.description}</p>
                        <p>Price: ${product.price}</p>
                        <p>Quantity: {product.quantity}</p>
                    </li>
                ))}
            </ul>
        </div>
    );
};

export default ProductList;
