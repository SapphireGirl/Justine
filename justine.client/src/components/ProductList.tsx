import { useEffect, useState } from "react";
import { LambdaClient, InvokeCommand } from '@aws-sdk/client-lambda';
import { ToastContainer, toast, Bounce } from 'react-toastify';
import { useNavigate } from "react-router-dom"; // Import useNavigate for navigation
import { fromCognitoIdentityPool } from "@aws-sdk/credential-providers";

// must be using AWS SDK v3 which means adding separate packages for each service
// See https://docs.aws.amazon.com/AWSJavaScriptSDK/v3/latest/client/cloudfront/
// Define the Product interface
interface Product {
    ProductId: string;
    name: string;
    description: string;
    price: number;
    quantity: number;
}

const ProductList = () => {
    const [products, setProducts] = useState<Product[]>([]); // Use the Product interface for type safety
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [userName, setUserName] = useState<string>(""); // State to hold the app user's name

    const navigate = useNavigate(); // Initialize useNavigate

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
            // Cognito: App must be deployed.
            // cognito login https://us-east-1pi7fvaqx0.auth.us-east-1.amazoncognito.com
            // need the JWT token
            // use examples: https://docs.aws.amazon.com/sdk-for-javascript/v3/developer-guide/loading-browser-credentials-cognito.html


            

            const lambdaClient = new LambdaClient({
                region: "us-east-1",
                credentials: fromCognitoIdentityPool({
                    clientConfig: { region: "us-east-1" },
                    identityPoolId: "us-east-1:f45d2cf0-3a60-41a8-ab6d-4a2b2378bac6", // Replace with your Cognito Identity Pool ID
                }),
            });

            const invokeLambda = async () => {
                const command = new InvokeCommand({
                    FunctionName: "GetAllProductsAsync",
                    InvocationType: "RequestResponse"
                    // GetAllProductsAsync does not require a payload
                });

                try {
                    const response = await lambdaClient.send(command);
                    let payloadString: string = ""; // Initialize payloadString to hold the response payload
                    let products: Product[] = []; // Initialize products array

                    //const products: Product[] = JSON.parse(response.Payload as string);
                    if (response.Payload) {
                        if (typeof response.Payload === "string") {
                            payloadString = response.Payload;
                        } else if (response.Payload instanceof Uint8Array) {
                            payloadString = new TextDecoder("utf-8").decode(response.Payload);
                        }
                    }

                    products = JSON.parse(payloadString);
                    setProducts(products);
                    console.log("Lambda response:", response);
                } catch (error) {
                    console.error("Error invoking Lambda:", error);
                    toast.error("Failed to fetch products.", {
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
                }
            };

            invokeLambda();


        };

        fetchProducts();

        // Simulate fetching the user's name (replace with actual logic)
        setUserName("Justine"); // Set the app user's name
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
            {userName === "Justine" && (
                <button onClick={() => navigate("/admin")}>Go to Admin</button> // Button to navigate to Admin
            )}
            <ul>
                {products.map((product) => (
                    <li key={product.ProductId}>
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
