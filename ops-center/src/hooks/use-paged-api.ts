import { useCallback, useEffect, useMemo, useState } from "react";
import { apiGet, buildPagedPath, type Paged } from "@/config/api";

export function usePagedApi<T>(basePath: string, defaults?: { pageSize?: number; extraParams?: Record<string, string | number | undefined> }) {
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(defaults?.pageSize ?? 20);
  const [extra, setExtra] = useState<Record<string, string | number | undefined>>(defaults?.extraParams ?? {});
  const [data, setData] = useState<Paged<T> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  const url = useMemo(() => buildPagedPath(basePath, page, pageSize, extra), [basePath, page, pageSize, extra]);

  const fetchData = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await apiGet<any>(url);
      // Normalize: backend may return array or paged
      if (Array.isArray(res)) {
        setData({ items: res, page: 1, pageSize: res.length, totalCount: res.length, totalPages: 1 });
      } else {
        setData({
          items: res?.items ?? [],
          page: res?.page ?? page,
          pageSize: res?.pageSize ?? pageSize,
          totalCount: res?.totalCount ?? 0,
          totalPages: res?.totalPages ?? 1,
        });
      }
    } catch (e) {
      setError(e as Error);
    } finally {
      setLoading(false);
    }
  }, [url, page, pageSize]);

  useEffect(() => { fetchData(); }, [fetchData]);

  const updateFilters = useCallback((next: Record<string, string | number | undefined>) => {
    setExtra(next);
    setPage(1);
  }, []);

  return {
    data,
    items: data?.items ?? [],
    loading,
    error,
    page,
    pageSize,
    totalCount: data?.totalCount ?? 0,
    totalPages: data?.totalPages ?? 1,
    setPage,
    setPageSize: (n: number) => { setPageSize(n); setPage(1); },
    setFilters: updateFilters,
    refetch: fetchData,
  };
}
