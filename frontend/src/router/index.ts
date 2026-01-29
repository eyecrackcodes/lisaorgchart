import { createRouter, createWebHistory } from "vue-router";
import OrgChartView from "@/views/OrgChartView.vue";

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: "/",
      redirect: "/org-chart",
    },
    {
      path: "/org-chart",
      name: "org-chart",
      component: OrgChartView,
      meta: {
        title: "Organization Chart",
      },
      children: [
        {
          path: "site/:siteId",
          name: "org-chart-site",
          component: () => import("@/views/OrgChartDrillView.vue"),
          props: true,
          meta: {
            title: "Site Details",
            drillType: "site",
          },
        },
        {
          path: "team/:teamId",
          name: "org-chart-team",
          component: () => import("@/views/OrgChartDrillView.vue"),
          props: true,
          meta: {
            title: "Team Details",
            drillType: "team",
          },
        },
        {
          path: "person/:personId",
          name: "org-chart-person",
          component: () => import("@/views/OrgChartDrillView.vue"),
          props: true,
          meta: {
            title: "Person Details",
            drillType: "person",
          },
        },
      ],
    },
    {
      path: "/reports",
      name: "reports",
      component: () => import("@/views/ReportsView.vue"),
      meta: {
        title: "Organization Reports",
      },
    },
  ],
});

// Update page title on navigation
router.beforeEach((to, from, next) => {
  document.title = to.meta.title
    ? `${to.meta.title} | LuminaryLife`
    : "LuminaryLife";
  next();
});

export default router;
