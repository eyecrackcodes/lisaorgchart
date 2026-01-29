<template>
  <div id="app">
    <nav class="main-nav">
      <div class="nav-brand">
        <span class="brand-icon">📊</span>
        <span class="brand-name">LuminaryLife</span>
      </div>
      <div class="nav-links">
        <RouterLink to="/org-chart" class="nav-link">
          <span class="nav-icon">🏢</span>
          Org Chart
        </RouterLink>
        <RouterLink to="/reports" class="nav-link">
          <span class="nav-icon">📈</span>
          Reports
        </RouterLink>
      </div>
    </nav>
    <main class="main-content">
      <RouterView />
    </main>
  </div>
</template>

<script setup lang="ts">
import { RouterLink, RouterView } from "vue-router";
</script>

<style>
/* Global Reset */
*,
*::before,
*::after {
  box-sizing: border-box;
}

body {
  margin: 0;
  font-family:
    -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Oxygen, Ubuntu,
    Cantarell, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  background: #f9fafb;
  color: #1f2937;
}

#app {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

/* Navigation */
.main-nav {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 24px;
  background: white;
  border-bottom: 1px solid #e5e7eb;
  position: sticky;
  top: 0;
  z-index: 100;
}

.nav-brand {
  display: flex;
  align-items: center;
  gap: 8px;
}

.brand-icon {
  font-size: 24px;
}

.brand-name {
  font-size: 1.25rem;
  font-weight: 700;
  color: #1f2937;
}

.nav-links {
  display: flex;
  gap: 16px;
}

.nav-link {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 16px;
  color: #4b5563;
  text-decoration: none;
  font-weight: 500;
  border-radius: 8px;
  transition: all 0.15s ease;
}

.nav-icon {
  font-size: 1rem;
}

.nav-link:hover {
  background: #f3f4f6;
  color: #1f2937;
}

.nav-link.router-link-active {
  background: #eff6ff;
  color: #3b82f6;
}

/* Main Content */
.main-content {
  flex: 1;
}
</style>
