import { defineStore } from "pinia"

export const useSidebarStore = defineStore('sidebar', {
  state: () => ({
    sidebarCollapsed: JSON.parse(localStorage.getItem('sidebarCollapsed') || 'false')
  }),

  actions: {
    setSidebarCollapsed(val) {
      this.sidebarCollapsed = val
      localStorage.setItem('sidebarCollapsed', JSON.stringify(val))
    }
  }
})
