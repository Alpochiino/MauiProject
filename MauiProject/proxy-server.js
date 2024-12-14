const express = require('express');
const { createProxyMiddleware } = require('http-proxy-middleware');

const app = express();

app.use('/geonames', createProxyMiddleware({
    target: 'http://api.geonames.org',
    changeOrigin: true,
    pathRewrite: {
        '^/geonames': '',
    },
}));

const PORT = 3000;
app.listen(PORT, () => {
    console.log(`Proxy server is running on http://localhost:${PORT}`);
});
