<script setup>
const emit = defineEmits(['close'])
</script>

<template>
  <div class="toast">
    <div class="toast__icon">✓</div>
    <span class="toast__msg">Successfully!</span>
    <div class="toast__close" @click="emit('close')">✕</div>
  </div>
</template>

<style scoped lang="scss">
.toast {
  position: fixed;
  bottom: 0.5rem;
  right: 0.5rem;
  z-index: 9999;
  display: flex;
  align-items: center;
  gap: 0.75rem;
  background: #fff;
  border: 1px solid #e5e7eb;
  border-left: 4px solid #16a34a;
  border-radius: 10px;
  padding: 0.75rem 1rem;
  min-width: 250px;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
  animation: slideIn 0.3s cubic-bezier(0.34, 1.56, 0.64, 1);

  &__icon {
    width: 24px;
    height: 24px;
    border-radius: 50%;
    background: #dcfce7;
    color: #16a34a;
    font-size: 13px;
    font-weight: 700;
    display: flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;
  }

  &__msg {
    font-size: 14px;
    font-weight: 500;
    color: #111827;
    flex: 1;
  }

  &__close {
    font-size: 12px;
    color: #9ca3af;
    cursor: pointer;
    padding: 2px 4px;
    border-radius: 4px;
    flex-shrink: 0;

    &:hover {
      background: #f3f4f6;
      color: #374151;
    }
  }
}

@keyframes slideIn {
  from {
    opacity: 0;
    transform: translateX(120%);
  }
  to {
    opacity: 1;
    transform: translateX(0);
  }
}
</style>
