const KEY = "ui-theme";

document.addEventListener("DOMContentLoaded", () => {
  const root = window.document.documentElement;
  root.classList.remove("light", "dark");

  let theme = localStorage.getItem(KEY) || "system";

  if (theme === "system") {
    theme = window.matchMedia("(prefers-color-scheme: dark)").matches
      ? "dark"
      : "light";
  }

  console.log(`setting theme: ${theme}`);
  root.classList.add(theme);
});