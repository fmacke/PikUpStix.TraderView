const { createProxyMiddleware } = require('http-proxy-middleware');

module.exports = function(app) {
  app.use(
    '/api',
    createProxyMiddleware({
      target: 'https://localhost:7001',
      changeOrigin: true,
      secure: false, // Allow self-signed certificates in development
    })
  );
};
