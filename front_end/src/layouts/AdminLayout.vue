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
