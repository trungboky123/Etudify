import { defineStore } from "pinia";

export const usePaymentStore = defineStore("payment", {
  state: () => ({
    info: null
  })
})
