<template>
  <el-dialog v-model="inputFile" width="500" :before-close="close"
    >
    <span>Потоковое добавление студентов с помощью excel-файла</span>
    <template #footer>
      <div class="dialog-footer">
        <el-upload
            ref="uploadRef"
            class="upload-demo"
            action="https://run.mocky.io/v3/9d059bf9-4660-45f2-925d-ce80ad6c4d15"
            :auto-upload="false"
        >
          <template #trigger>
            <el-button type="primary">Выбрать файл</el-button>
          </template>
          <template #tip>
            <el-button>Скачать файл формата: </el-button>
          </template>
        </el-upload>
        <el-button @click="cancel">Нет</el-button>
        <el-button type="primary" @click="confirm">Да</el-button>
      </div>
    </template>
    
  </el-dialog>
  <div>
    <el-button @click = "Create" type = "button" v-if="props.createVisibility" class = "add">Добавить</el-button>
    <el-button @click = "inputFile=true" type = "button" v-if = "props.createVisibility" class = "addFromInputFile add">Загрузить файлом(excel)</el-button>
  </div>
  <el-table-v2
      :columns="CreateColumns"
      :data="CreateData"
      :width="viewportWidth"
      :height="viewportHeight*0.60"
      fixed
      :gutter = "1"
      :row-height = props.height
      :sort-by = "sortState"
      @column-sort = "OnSort"
      :row-event-handlers="OpenPage"
  >     
    <template #empty>
      <div class="flex items-center justify-center h-100%">
        <el-empty 
          description="Нет данных(проверьте подключение к сети)"
        />
      </div>
    </template>
  </el-table-v2>
  <div class = "pageButtons">
    <el-button v-for = "n in props.countPage" circle @click = "PageSet(n)">{{n}}</el-button>
  </div>
</template>
<style scoped>
  .pageButtons {
    display: flex;
    justify-content: center;
    flex-wrap: wrap;
  }
  .add{
    margin-left: 8px;
  }
  #footer{
    
  }
</style>  
<script lang="ts" setup>
import type {SortType, TableColumn, TableData} from '@/types/TableTypes.ts';
import {computed, onMounted, reactive, ref} from "vue";
import router from '@/router/index.ts';
import {useRoute} from 'vue-router';
import type {SortBy} from 'element-plus'
import {SortOrder} from "element-plus/es/components/table-v2/src/constants";
import type { RowEventHandlers  } from 'element-plus'
  const inputFile = ref<boolean>(false);
  const viewportWidth = window.innerWidth;
  const viewportHeight = window.innerHeight;
  const route = useRoute();
  const props = defineProps <{
        defaultSortKey: string,
        columns: TableColumn[],
        height: number,
        data: TableData[],
        apiBase: string,
        countPage: number;
        createVisibility: boolean
      }> ();
const sortState = ref<SortBy>({
  key: props.defaultSortKey,
  order: SortOrder.ASC,
})
  const CreateColumns = computed(() => {
    return props.columns.map((column) => {
    return {
      ...column,
      width: column.width ?? 0.1 * viewportWidth,
      sortable: column.sortable ?? true
    }});
  })
  const CreateData = computed(() =>
  {
    if (!props.data || !props.columns) {
      return []
    }
    return props.data.map((datarow, keyForData) => {
      const row: TableData  = {
        id: `${datarow.id || keyForData}`,
      }
      props.columns.forEach(((column, index) => {
        if(datarow[column.dataKey] !== undefined) {
          if(column.type == 'string' || column.type == 'number')
          {
            row[column.dataKey] = datarow[column.dataKey]
          }
          else if(column.type == 'date')
          {
            row[column.dataKey] = Intl.DateTimeFormat("ru-RU").format(new Date(datarow[column.dataKey]));
          }
        }
        else {
          const keys = Object.keys(datarow);
          if (keys.length > 0) {
            const dataIndex = index < keys.length ? index : 0;
            const key = keys[dataIndex];
            if(key && datarow[key] !== undefined) {
              if(column.type == 'string' || column.type == 'number')
              {
                row[column.dataKey] = datarow[key]
              }
              else if(column.type == 'date')
              {
                row[column.dataKey] = Intl.DateTimeFormat("ru-RU").format(datarow[key]);
              }
            }
          } else {    
            row[column.dataKey] = '';
          }
        }
      }))
      return row;
    })
  });
  const PageSet = ((n : number) =>
  {
    router.push({query: {...route.query, NumberPage: n}});
  })
  const OnSort = (sortBy: SortBy) =>
  {
    sortState.value = sortBy;
    router.push({query: {...route.query, sortKey: sortState.value.key.toString(), sortType: sortState.value.order.toString()}});
  }
  const OpenPage : RowEventHandlers = {
    onClick: (row: any) =>
    {
      let id = row.rowData.id;
      let x = props.data[id];
      if(x !== undefined) {
        id = x.studentId;
      }
      debugger;
      router.push(`${props.apiBase}/${id}`);
    }
  }
const Create = () => {
  router.push(`${props.apiBase}/${undefined}`);
}
const emit = defineEmits<
    {
      (e: 'update:modelValue', value: boolean): void
      (e: 'confirm'): void
      (e: 'cancel'): void
      (e: 'close'): void
    }>();
const cancel = () => {
  inputFile.value = false
  emit('cancel');
};
const close = (done : () => void) => {
  inputFile.value = false
  emit('close');
  done();
};
const confirm = () => {
  inputFile.value = false
  emit("confirm");
}
</script>   