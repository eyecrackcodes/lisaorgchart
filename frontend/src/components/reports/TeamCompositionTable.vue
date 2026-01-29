<template>
  <div class="chart-card">
    <div class="chart-header">
      <h3 class="chart-title">Team Composition</h3>
      <input 
        v-model="search" 
        type="text" 
        placeholder="Search teams..." 
        class="search-input"
      />
    </div>
    
    <div class="table-container">
      <table class="team-table">
        <thead>
          <tr>
            <th @click="sortBy('teamName')" class="sortable">
              Team
              <span class="sort-icon" :class="{ active: sortKey === 'teamName' }">↕</span>
            </th>
            <th @click="sortBy('siteName')" class="sortable">
              Site
              <span class="sort-icon" :class="{ active: sortKey === 'siteName' }">↕</span>
            </th>
            <th @click="sortBy('memberCount')" class="sortable num">
              Size
              <span class="sort-icon" :class="{ active: sortKey === 'memberCount' }">↕</span>
            </th>
            <th @click="sortBy('activeCount')" class="sortable num">
              Active
              <span class="sort-icon" :class="{ active: sortKey === 'activeCount' }">↕</span>
            </th>
            <th class="tiers">Tier Mix</th>
            <th @click="sortBy('averageTenure')" class="sortable num">
              Tenure
              <span class="sort-icon" :class="{ active: sortKey === 'averageTenure' }">↕</span>
            </th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="team in sortedTeams" :key="team.teamId">
            <td>
              <div class="team-cell">
                <span class="team-name">{{ team.teamName }}</span>
                <span class="manager-name">{{ team.managerName }}</span>
              </div>
            </td>
            <td>
              <span class="site-badge">{{ team.siteName }}</span>
            </td>
            <td class="num">{{ team.memberCount }}</td>
            <td class="num">
              <span class="active-badge" :class="getActiveClass(team)">
                {{ team.activeCount }}
              </span>
            </td>
            <td class="tiers">
              <div class="tier-bars">
                <div 
                  class="tier-segment tier1" 
                  :style="{ width: `${team.tier1Percent}%` }"
                  :title="`Tier 1: ${Math.round(team.tier1Percent)}%`"
                ></div>
                <div 
                  class="tier-segment tier2" 
                  :style="{ width: `${team.tier2Percent}%` }"
                  :title="`Tier 2: ${Math.round(team.tier2Percent)}%`"
                ></div>
                <div 
                  class="tier-segment tier3" 
                  :style="{ width: `${team.tier3Percent}%` }"
                  :title="`Tier 3: ${Math.round(team.tier3Percent)}%`"
                ></div>
              </div>
            </td>
            <td class="num">
              <span class="tenure-value">{{ formatTenure(team.averageTenure) }}</span>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import type { TeamCompositionData } from '@/types/reports';

const props = defineProps<{
  teams: TeamCompositionData[];
}>();

const search = ref('');
const sortKey = ref<keyof TeamCompositionData>('teamName');
const sortAsc = ref(true);

const filteredTeams = computed(() => {
  if (!search.value) return props.teams;
  const term = search.value.toLowerCase();
  return props.teams.filter(t => 
    t.teamName.toLowerCase().includes(term) ||
    t.siteName.toLowerCase().includes(term) ||
    t.managerName.toLowerCase().includes(term)
  );
});

const sortedTeams = computed(() => {
  const sorted = [...filteredTeams.value];
  sorted.sort((a, b) => {
    const aVal = a[sortKey.value];
    const bVal = b[sortKey.value];
    
    if (typeof aVal === 'string' && typeof bVal === 'string') {
      return sortAsc.value 
        ? aVal.localeCompare(bVal) 
        : bVal.localeCompare(aVal);
    }
    
    return sortAsc.value 
      ? (aVal as number) - (bVal as number) 
      : (bVal as number) - (aVal as number);
  });
  return sorted;
});

function sortBy(key: keyof TeamCompositionData) {
  if (sortKey.value === key) {
    sortAsc.value = !sortAsc.value;
  } else {
    sortKey.value = key;
    sortAsc.value = true;
  }
}

function getActiveClass(team: TeamCompositionData): string {
  const percent = team.memberCount > 0 ? team.activeCount / team.memberCount : 0;
  if (percent >= 0.9) return 'high';
  if (percent >= 0.7) return 'medium';
  return 'low';
}

function formatTenure(years: number): string {
  if (years < 1) return `${Math.round(years * 12)}mo`;
  return `${years.toFixed(1)}yr`;
}
</script>

<style scoped>
.chart-card {
  background: white;
  border-radius: 16px;
  padding: 24px;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.06);
}

.chart-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.chart-title {
  font-size: 1.1rem;
  font-weight: 700;
  color: #1f2937;
  margin: 0;
}

.search-input {
  padding: 8px 14px;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  font-size: 0.85rem;
  width: 200px;
}

.search-input:focus {
  outline: none;
  border-color: #3b82f6;
}

.table-container {
  overflow-x: auto;
}

.team-table {
  width: 100%;
  border-collapse: collapse;
}

.team-table th,
.team-table td {
  padding: 12px 16px;
  text-align: left;
  border-bottom: 1px solid #e5e7eb;
}

.team-table th {
  font-size: 0.75rem;
  font-weight: 600;
  color: #6b7280;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  background: #f9fafb;
}

.team-table th.sortable {
  cursor: pointer;
  user-select: none;
}

.team-table th.sortable:hover {
  color: #3b82f6;
}

.sort-icon {
  opacity: 0.3;
  margin-left: 4px;
}

.sort-icon.active {
  opacity: 1;
  color: #3b82f6;
}

.team-table th.num,
.team-table td.num {
  text-align: center;
}

.team-table th.tiers,
.team-table td.tiers {
  width: 150px;
}

.team-table tbody tr:hover {
  background: #f9fafb;
}

.team-cell {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.team-name {
  font-weight: 600;
  color: #1f2937;
}

.manager-name {
  font-size: 0.75rem;
  color: #9ca3af;
}

.site-badge {
  display: inline-block;
  padding: 4px 10px;
  background: #eff6ff;
  color: #3b82f6;
  border-radius: 12px;
  font-size: 0.75rem;
  font-weight: 500;
}

.active-badge {
  display: inline-block;
  padding: 4px 10px;
  border-radius: 12px;
  font-size: 0.85rem;
  font-weight: 600;
}

.active-badge.high { background: #dcfce7; color: #166534; }
.active-badge.medium { background: #fef3c7; color: #92400e; }
.active-badge.low { background: #fee2e2; color: #991b1b; }

.tier-bars {
  display: flex;
  height: 10px;
  border-radius: 5px;
  overflow: hidden;
  background: #e5e7eb;
}

.tier-segment {
  height: 100%;
  transition: width 0.3s ease;
}

.tier-segment.tier1 { background: #22c55e; }
.tier-segment.tier2 { background: #3b82f6; }
.tier-segment.tier3 { background: #8b5cf6; }

.tenure-value {
  font-weight: 500;
  color: #4b5563;
}
</style>
