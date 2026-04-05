import {
  defineStore
} from "pinia";
import authAxios from '@/function/authAxios'

export const useAuthStore = defineStore("auth", {
  state: () => ({
    user: null,
    isLoaded: false
  }),

  getters: {
    isAuthenticated: (state) => !!state.user,
    role: (state) => state.user?.role
  },

  actions: {
    async fetchUser() {
      try {
        const res = await authAxios.get("/users/me");
        this.user = res.data;
      } catch (error) {
        this.user = null;
      } finally {
        this.isLoaded = true;
      }
    }
  }
})

