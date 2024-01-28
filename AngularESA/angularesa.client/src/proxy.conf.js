const PROXY_CONFIG = [
  {
    context: [
      "/weatherforecast",
      "/api",
      "/login",
      "/logout",
      "/register",
      "/refresh",
      "/confirmEmail",
      "/resendConfirmationEmail",
      "/forgotPassword",
      "/resetPassword",
      "/manage"
    ],
    target: "https://localhost:7041",
    secure: false
  }
]

module.exports = PROXY_CONFIG;
