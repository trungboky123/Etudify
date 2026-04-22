<script setup>
import AdminHeader from '@/components/AdminHeader.vue';
import AdminSidebar from '@/components/AdminSidebar.vue';
import { useSidebarStore } from '@/stores/sidebar';

const sidebar = useSidebarStore();
</script>

<template>
  <AdminHeader :sidebarCollapsed="sidebar.sidebarCollapsed" />
  <AdminSidebar :sidebarCollapsed="sidebar.sidebarCollapsed" @collapse="sidebar.setSidebarCollapsed"/>
  <RouterView :sidebarCollapsed="sidebar.sidebarCollapsed" />
</template>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@400;500;600;700;800&display=swap');
</style>
