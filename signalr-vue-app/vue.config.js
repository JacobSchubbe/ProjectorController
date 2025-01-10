const { defineConfig } = require('@vue/cli-service')
const appPort = process.env.VUE_APP_PORT || '8080';
module.exports = defineConfig({
  publicPath: '/',
  transpileDependencies: true,
  devServer: {
    host: '0.0.0.0',  // This allows external devices to access it
    port: appPort,       // Make sure this matches the port you want to use
    allowedHosts: 'all',  // Allow all hosts, replacing the need for disableHostCheck
  }
})