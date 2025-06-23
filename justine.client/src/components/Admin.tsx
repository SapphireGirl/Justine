import React from "react";
//import { CognitoIdentityProviderClient, InitiateAuthCommand, AuthFlowType } from "@aws-sdk/client-cognito-identity-provider";
import { LambdaClient, InvokeCommand } from '@aws-sdk/client-lambda';
import { ToastContainer, toast, Bounce } from 'react-toastify';


interface AdminProps {
    userName: string;
}

const Admin: React.FC<AdminProps> = ({ userName }) => {
    //const invokeLambda = async (functionName: string, payload: object) => {

    //    // Add the User's Id Token to the Cognito credentials login map.
    //    // Authentication: Verigying the user's identity
    //    // Authorization: Verifying what the user can do
    //    // Role-based Access Control (RBAC): Users are divided into groups.  Users are able to be assigned to a group as part of their identity (authentication)
    //    // and permissions are assigned for each groups (authorization).
    //    // Cognito User Pools: Authentication
    //    // Identity Pools: federated identities that are allowed to obtain temporary AWS credentials based on a specified IAM role.

    const lambdaClient = new LambdaClient({ region: "us-east-1" }); 

    const invokeCreateProductTableAsyncLambda = async () => {
        const command = new InvokeCommand({
            FunctionName: "CreateProductTableAsync",
            InvocationType: "RequestResponse"
            // GetAllProductsAsync does not require a payload
        });

        try {
            const response = await lambdaClient.send(command);
            
            

            console.log(`Lambda response: ${JSON.stringify(response)}`);
        } catch (error) {
            console.error(`Error invoking Lambda: CreateProductTableAsync: ${JSON.stringify(error)}`);
            toast.error("Failed to Create Product Table.", {
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

    const invokeCreateBasketTableAsyncLambda = async () => {
        const command = new InvokeCommand({
            FunctionName: "CreateBasketTableAsync",
            InvocationType: "RequestResponse"
            // GetAllProductsAsync does not require a payload
        });

        try {
            const response = await lambdaClient.send(command);



            console.log(`Lambda response: ${JSON.stringify(response)}`);
        } catch (error) {
            console.error(`Error invoking Lambda: CreateBasketTableAsync: ${JSON.stringify(error)}`);
            toast.error("Failed to Create Basket Table.", {
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

    const invokeCreateOrderTableAsyncLambda = async () => {
        const command = new InvokeCommand({
            FunctionName: "CreateOrderTableAsync",
            InvocationType: "RequestResponse"
            // GetAllProductsAsync does not require a payload
        });

        try {
            const response = await lambdaClient.send(command);



            console.log(`Lambda response: ${JSON.stringify(response)}`);
        } catch (error) {
            console.error(`Error invoking Lambda: CreateOrderTableAsync: ${JSON.stringify(error)}`);
            toast.error("Failed to Create Order Table.", {
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

    const invokeDeleteTablesAsyncLambda = async () => {
        const command = new InvokeCommand({
            FunctionName: "DeleteTablesAsync",
            InvocationType: "RequestResponse"
            // GetAllProductsAsync does not require a payload
        });

        try {
            const response = await lambdaClient.send(command);



            console.log(`Lambda response: ${JSON.stringify(response)}`);
        } catch (error) {
            console.error(`Error invoking Lambda: DeleteTablesAsync: ${JSON.stringify(error)}`);
            toast.error("Failed to Delete Tables.", {
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

    const invokeSeedDatabaseAsyncLambda = async () => {
        const command = new InvokeCommand({
            FunctionName: "SeedDatabaseAsync",
            InvocationType: "RequestResponse"
            // GetAllProductsAsync does not require a payload
        });

        try {
            const response = await lambdaClient.send(command);



            console.log(`Lambda response: ${JSON.stringify(response)}`);
        } catch (error) {
            console.error(`Error invoking Lambda: SeedDatabaseAsync: ${JSON.stringify(error)}`);
            toast.error("Failed to Seed Database Tables.", {
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

    const invokeBackupDatabaseAsyncLambda = async () => {
        const command = new InvokeCommand({
            FunctionName: "BackupDatabaseAsync",
            InvocationType: "RequestResponse"
            // GetAllProductsAsync does not require a payload
        });

        try {
            const response = await lambdaClient.send(command);



            console.log(`Lambda response: ${JSON.stringify(response)}`);
        } catch (error) {
            console.error(`Error invoking Lambda: BackupDatabaseAsync: ${JSON.stringify(error)}`);
            toast.error("Failed to Create Product Table.", {
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

    const invokeRestoreDatabaseAsyncLambda = async () => {
        const command = new InvokeCommand({
            FunctionName: "RestoreDatabaseAsync",
            InvocationType: "RequestResponse"
            // GetAllProductsAsync does not require a payload
        });

        try {
            const response = await lambdaClient.send(command);



            console.log(`Lambda response: ${JSON.stringify(response)}`);
        } catch (error) {
            console.error(`Error invoking Lambda: RestoreDatabaseAsync: ${JSON.stringify(error)}`);
            toast.error("Failed to Create Product Table.", {
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


    if (userName === "Justine") {
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
                <h1>Admin Actions</h1>
                <button onClick={() =>
                    invokeCreateProductTableAsyncLambda()}>
                    Create Product Table and Populate
                </button>
                <button onClick={() => invokeCreateBasketTableAsyncLambda()}>
                    Create Basket Table and Populate
                </button>
                <button onClick={() => invokeCreateOrderTableAsyncLambda()}>
                    Create Order Table and Populate
                </button>
                <button onClick={() => invokeDeleteTablesAsyncLambda()}>
                    Delete Tables
                </button>
                <button onClick={() => invokeSeedDatabaseAsyncLambda()}>
                    Seed Database
                </button>
                <button onClick={() => invokeBackupDatabaseAsyncLambda()}>
                    Backup Database
                </button>
                <button onClick={() => invokeRestoreDatabaseAsyncLambda()}>
                    Restore Database
                </button>
            </div>
        );
    }
    return null; // Do not render anything if the user is not "Justine"
};

export default Admin;
