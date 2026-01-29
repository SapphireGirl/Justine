# from repo root
cd justine.client
npm install
npm run build
# copy output into ASP.NET project wwwroot
rm -rf ../Justine.LambdaWebApi/wwwroot/*
cp -r dist/* ../Justine.LambdaWebApi/wwwroot/