import { fileURLToPath, URL } from 'node:url';
import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';
import fs from 'fs';
import path from 'path';
import child_process from 'child_process';
import { env } from 'process';

const baseFolder = './ssl/'
//    env.APPDATA !== undefined && env.APPDATA !== ''
//        ? `${env.APPDATA}/ASP.NET/https`
//        : `${env.HOME}/.aspnet/https`;

const certificateName = "JustineClient";
const certFilePath = path.join(baseFolder, `${certificateName}.crt`);
const keyFilePath = path.join(baseFolder, `${certificateName}.key`);
const pfxFilePath = path.join(baseFolder, `${certificateName}.pfx`);
const tempPass = 'tempPass';

if (!fs.existsSync(certFilePath) || !fs.existsSync(keyFilePath)) {
    if (0 !== child_process.spawnSync('dotnet', [
        'dev-certs',
        'https',
        '--export-path',
        pfxFilePath,
        '-p',
        tempPass,
    ], { stdio: 'inherit', }).status) {
        throw new Error("Could not create certificate.");
    }

    child_process.execSync(`openssl pkcs12 -in ${pfxFilePath} -clcerts -nokeys -out ${certFilePath} -passin pass:${tempPass}`);
    child_process.execSync(`openssl pkcs12 -in ${pfxFilePath} -nocerts -nodes -out ${keyFilePath} -passin pass:${tempPass}`);
}

const pem = fs.readFileSync(certFilePath, 'utf8');
const certMatch = pem.match(/-----BEGIN CERTIFICATE-----[\\s\\S]+?-----END CERTIFICATE-----/);
const keyMatch  = pem.match(/-----BEGIN (?:RSA )?PRIVATE KEY-----[\\s\\S]+?-----END (?:RSA )?PRIVATE KEY-----/);

console.log(`Certificate generated at: ${certFilePath}`);
console.log(`Key generated at: ${keyFilePath}`);
console.log('ASPNETCORE_URLS is set to: ' + env.ASPNETCORE_URLS); 
console.log('Port: ' + env.ASPNETCORE_HTTPS_PORT); 

console.log('certMatch present:', !!certMatch);
console.log('keyMatch present:', !!keyMatch);
if (!certMatch || !keyMatch) {
  console.error('TLS cert or key not found or not parsed. Check ssl files and extraction.');
}

const target = env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}` :
    env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'https://localhost:7259';

// https://vitejs.dev/config/
export default defineConfig({
    
    plugins: [plugin()],
    resolve: {
        alias: {
            '@': fileURLToPath(new URL('./src', import.meta.url))
        }
    },
    server: {
        proxy: {
            '^/login': {
                target,
                secure: false
            }
        },
        port: 5173,
        https: {
            key: fs.readFileSync(path.resolve(__dirname, 'ssl/JustineClient.key'), 'utf8'),
            cert: fs.readFileSync(path.resolve(__dirname, 'ssl/JustineClient.crt'), 'utf8'),
        },
    }
})
