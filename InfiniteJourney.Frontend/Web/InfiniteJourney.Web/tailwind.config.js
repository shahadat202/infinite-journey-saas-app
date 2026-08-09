/** @type {import('tailwindcss').Config} */
export default {
  content: [
    './src/**/*.{html,ts}',
  ],
  theme: {
    extend: {
      colors: {
        primary: 'var(--primary)',
        secondary: 'var(--secondary)',
        accent: 'var(--accent)',
        text: 'var(--text)',
        muted: 'var(--muted)',
        surface: 'var(--surface)',
        border: 'var(--border)',
        background: 'var(--background)',
      },
    },
  },
  plugins: [],
}
