<script setup>
const emit = defineEmits(['cancel', 'confirm'])
const props = defineProps({
  type: String,
  activation: Boolean,
  id: String
})
</script>

<template>
  <div class="backdrop" @click.self="emit('cancel')">
    <div class="dialog">
      <div class="icon">⚠️</div>
      <h2 class="title">Warning!</h2>
      <p class="desc">
        Do you want to <span v-if="props.activation">activate</span><span v-else>deactivate</span> this
        <span v-if="props.type === 'Account'">account</span><span v-else>course</span>?
      </p>
      <div class="actions">
        <div class="btn-cancel" @click="emit('cancel')">Cancel</div>
        <div class="btn-delete" @click="emit('confirm', props.id)">
          <span v-if="props.activation">Activate</span><span v-else>Deactivate</span>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.backdrop {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 9999;
  animation: fadeIn 0.2s ease;
}

.dialog {
  background: #fff;
  border-radius: 12px;
  padding: 2rem 1.75rem 1.5rem;
  width: 100%;
  max-width: 360px;
  text-align: center;
  animation: popIn 0.25s cubic-bezier(0.34, 1.56, 0.64, 1);
}

.icon {
  font-size: 2rem;
  margin-bottom: 0.75rem;
}

.title {
  font-size: 17px;
  font-weight: 600;
  color: #111827;
  margin: 0 0 0.5rem;
}

.desc {
  font-size: 14px;
  color: #6b7280;
  margin: 0 0 1.5rem;
  line-height: 1.6;
}

.actions {
  display: flex;
  gap: 0.6rem;
}

.btn-cancel,
.btn-delete {
  flex: 1;
  height: 40px;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
}

.btn-cancel {
  background: #f3f4f6;
  color: #374151;
}

.btn-cancel:hover {
  background: #e5e7eb;
}

.btn-delete {
  background: #dc2626;
  color: #fff;
}

.btn-delete:hover {
  background: #b91c1c;
}

@keyframes fadeIn {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}

@keyframes popIn {
  from {
    opacity: 0;
    transform: scale(0.85);
  }
  to {
    opacity: 1;
    transform: scale(1);
  }
}
</style>
