/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["./src/**/*.{html,ts}"],
  theme: {
    extend: {
      colors: {
        "brand-blue": "#0057cd",
        "brand-green": "#006c4f",
        "surface-gray": "#f8f9fa",
      },
      fontFamily: {
        headline: ["Manrope", "sans-serif"],
        body: ["Inter", "sans-serif"],
      },
      borderRadius: {
        medical: "0.5rem",
      },
    },
  },
  plugins: [],
};
