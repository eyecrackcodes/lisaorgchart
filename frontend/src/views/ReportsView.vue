<template>
  <div class="reports-view">
    <!-- Header -->
    <header class="page-header">
      <div class="header-content">
        <h1 class="page-title">Organization Reports</h1>
        <p class="page-subtitle">Analytics and insights for your organization</p>
      </div>
      <div class="header-actions">
        <button class="action-btn" @click="exportReport">
          <span class="icon">📊</span>
          Export PDF
        </button>
        <button class="action-btn primary" @click="refreshData">
          <span v-if="loading" class="btn-spinner"></span>
          Refresh
        </button>
      </div>
    </header>

    <!-- Loading State -->
    <div v-if="loading && !reportData" class="loading-state">
      <div class="spinner"></div>
      <span>Loading report data...</span>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="error-state">
      <span class="error-icon">⚠️</span>
      <p>{{ error.message }}</p>
      <button @click="loadReportData" class="retry-btn">Retry</button>
    </div>

    <!-- Report Content -->
    <template v-else-if="reportData">
      <!-- Summary Cards -->
      <section class="report-section">
        <SummaryCards :summary="reportData.summary" />
      </section>

      <!-- Charts Grid -->
      <section class="report-section charts-grid">
        <div class="chart-row two-col">
          <TierDistributionChart :data="reportData.tierDistribution" />
          <TenureDistributionChart :data="reportData.tenureDistribution" />
        </div>

        <div class="chart-row single">
          <SiteComparisonChart :sites="reportData.siteComparison" />
        </div>

        <div class="chart-row single">
          <HierarchyChart :data="reportData.hierarchyDepth" />
        </div>

        <div class="chart-row single">
          <TeamCompositionTable :teams="reportData.teamComposition" />
        </div>
      </section>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { fetchReportData } from '@/api/reports';
import type { OrgReportData } from '@/types/reports';
import SummaryCards from '@/components/reports/SummaryCards.vue';
import TierDistributionChart from '@/components/reports/TierDistributionChart.vue';
import TenureDistributionChart from '@/components/reports/TenureDistributionChart.vue';
import SiteComparisonChart from '@/components/reports/SiteComparisonChart.vue';
import HierarchyChart from '@/components/reports/HierarchyChart.vue';
import TeamCompositionTable from '@/components/reports/TeamCompositionTable.vue';

const reportData = ref<OrgReportData | null>(null);
const loading = ref(false);
const error = ref<Error | null>(null);

async function loadReportData() {
  loading.value = true;
  error.value = null;
  
  try {
    reportData.value = await fetchReportData();
  } catch (e) {
    error.value = e as Error;
    console.error('Failed to load report data:', e);
  } finally {
    loading.value = false;
  }
}

async function refreshData() {
  await loadReportData();
}

function exportReport() {
  console.log('Exporting report...');
  // TODO: Implement PDF export
  alert('Export functionality coming soon!');
}

onMounted(() => {
  loadReportData();
});
</script>

<style scoped>
.reports-view {
  min-height: 100vh;
  background: linear-gradient(180deg, #f8fafc 0%, #f1f5f9 100%);
  padding: 24px;
}

/* Header */
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 32px;
  flex-wrap: wrap;
  gap: 16px;
}

.header-content {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.page-title {
  font-size: 2rem;
  font-weight: 800;
  color: #1f2937;
  margin: 0;
  background: linear-gradient(90deg, #1f2937 0%, #3b82f6 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.page-subtitle {
  font-size: 0.95rem;
  color: #6b7280;
  margin: 0;
}

.header-actions {
  display: flex;
  gap: 12px;
}

.action-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 18px;
  background: white;
  border: 1px solid #e5e7eb;
  border-radius: 10px;
  font-size: 0.9rem;
  font-weight: 500;
  color: #4b5563;
  cursor: pointer;
  transition: all 0.15s ease;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.04);
}

.action-btn:hover {
  background: #f9fafb;
  border-color: #d1d5db;
  transform: translateY(-1px);
}

.action-btn.primary {
  background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
  border-color: transparent;
  color: white;
}

.action-btn.primary:hover {
  background: linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%);
}

.action-btn .icon {
  font-size: 1rem;
}

.btn-spinner {
  width: 14px;
  height: 14px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-top-color: white;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

/* States */
.loading-state,
.error-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 80px;
  gap: 16px;
  color: #6b7280;
}

.spinner {
  width: 48px;
  height: 48px;
  border: 3px solid #e5e7eb;
  border-top-color: #3b82f6;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.error-icon {
  font-size: 56px;
}

.retry-btn {
  padding: 10px 28px;
  background: #3b82f6;
  color: white;
  border: none;
  border-radius: 8px;
  font-weight: 500;
  cursor: pointer;
}

.retry-btn:hover {
  background: #2563eb;
}

/* Report Sections */
.report-section {
  margin-bottom: 32px;
}

.charts-grid {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.chart-row {
  display: grid;
  gap: 24px;
}

.chart-row.two-col {
  grid-template-columns: repeat(2, 1fr);
}

.chart-row.single {
  grid-template-columns: 1fr;
}

/* Responsive */
@media (max-width: 1024px) {
  .chart-row.two-col {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 768px) {
  .reports-view {
    padding: 16px;
  }

  .page-header {
    flex-direction: column;
    align-items: flex-start;
  }

  .page-title {
    font-size: 1.5rem;
  }
}
</style>
