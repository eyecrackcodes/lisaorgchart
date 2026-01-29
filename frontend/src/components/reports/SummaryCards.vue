<template>
  <div class="summary-cards">
    <div class="card primary">
      <div class="card-icon">👥</div>
      <div class="card-content">
        <span class="card-value">{{ summary.totalAgents }}</span>
        <span class="card-label">Total Agents</span>
      </div>
      <div class="card-footer">
        <span class="active">{{ summary.activeAgents }} active</span>
        <span class="inactive">{{ summary.inactiveAgents }} inactive</span>
      </div>
    </div>

    <div class="card success">
      <div class="card-icon">✓</div>
      <div class="card-content">
        <span class="card-value">{{ activePercent }}%</span>
        <span class="card-label">Active Rate</span>
      </div>
      <div class="card-progress">
        <div class="progress-bar" :style="{ width: `${activePercent}%` }"></div>
      </div>
    </div>

    <div class="card warning">
      <div class="card-icon">🎓</div>
      <div class="card-content">
        <span class="card-value">{{ summary.trainingAgents }}</span>
        <span class="card-label">In Training</span>
      </div>
      <div class="card-footer">
        <span>{{ trainingPercent }}% of total</span>
      </div>
    </div>

    <div class="card info">
      <div class="card-icon">👔</div>
      <div class="card-content">
        <span class="card-value">{{ summary.totalManagers }}</span>
        <span class="card-label">Managers</span>
      </div>
      <div class="card-footer">
        <span>{{ spanOfControl }} avg reports</span>
      </div>
    </div>

    <div class="card secondary">
      <div class="card-icon">🏢</div>
      <div class="card-content">
        <span class="card-value">{{ summary.totalSites }}</span>
        <span class="card-label">Sites</span>
      </div>
      <div class="card-footer">
        <span>{{ summary.totalTeams }} teams</span>
      </div>
    </div>

    <div class="card accent">
      <div class="card-icon">📅</div>
      <div class="card-content">
        <span class="card-value">{{ formatTenure(summary.averageTenure) }}</span>
        <span class="card-label">Avg Tenure</span>
      </div>
      <div class="card-footer">
        <span>{{ formatTeamSize(summary.averageTeamSize) }} avg team</span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import type { OrgSummary } from '@/types/reports';

const props = defineProps<{
  summary: OrgSummary;
}>();

const activePercent = computed(() => 
  props.summary.totalAgents > 0 
    ? Math.round((props.summary.activeAgents / props.summary.totalAgents) * 100) 
    : 0
);

const trainingPercent = computed(() => 
  props.summary.totalAgents > 0 
    ? Math.round((props.summary.trainingAgents / props.summary.totalAgents) * 100) 
    : 0
);

const spanOfControl = computed(() => {
  if (props.summary.totalManagers === 0) return 0;
  return Math.round(props.summary.totalAgents / props.summary.totalManagers);
});

function formatTenure(years: number): string {
  if (years < 1) {
    return `${Math.round(years * 12)}mo`;
  }
  return `${years.toFixed(1)}yr`;
}

function formatTeamSize(size: number): string {
  return size.toFixed(1);
}
</script>

<style scoped>
.summary-cards {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  gap: 20px;
}

.card {
  background: white;
  border-radius: 16px;
  padding: 24px;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.06);
  display: flex;
  flex-direction: column;
  gap: 12px;
  position: relative;
  overflow: hidden;
}

.card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 4px;
}

.card.primary::before { background: linear-gradient(90deg, #3b82f6, #60a5fa); }
.card.success::before { background: linear-gradient(90deg, #22c55e, #4ade80); }
.card.warning::before { background: linear-gradient(90deg, #f59e0b, #fbbf24); }
.card.info::before { background: linear-gradient(90deg, #0ea5e9, #38bdf8); }
.card.secondary::before { background: linear-gradient(90deg, #8b5cf6, #a78bfa); }
.card.accent::before { background: linear-gradient(90deg, #ec4899, #f472b6); }

.card-icon {
  font-size: 28px;
  opacity: 0.9;
}

.card-content {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.card-value {
  font-size: 2rem;
  font-weight: 800;
  color: #1f2937;
  line-height: 1;
}

.card-label {
  font-size: 0.85rem;
  color: #6b7280;
  font-weight: 500;
}

.card-footer {
  display: flex;
  gap: 12px;
  font-size: 0.75rem;
  color: #9ca3af;
  margin-top: auto;
}

.card-footer .active {
  color: #22c55e;
}

.card-footer .inactive {
  color: #6b7280;
}

.card-progress {
  height: 6px;
  background: #e5e7eb;
  border-radius: 3px;
  overflow: hidden;
  margin-top: auto;
}

.progress-bar {
  height: 100%;
  background: linear-gradient(90deg, #22c55e, #4ade80);
  border-radius: 3px;
  transition: width 0.5s ease;
}
</style>
