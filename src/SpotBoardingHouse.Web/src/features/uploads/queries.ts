import { useMutation } from "@tanstack/react-query";
import { api } from "../../lib/api";
import type { MediaAssetUploadResultDto } from "./types";

export function useUploadImage() {
  return useMutation({
    mutationFn: async (file: File) => {
      const formData = new FormData();
      formData.append("File", file);

      const { data } = await api.post<MediaAssetUploadResultDto>(
        "api/v1/uploads/images",
        formData,
        {
          headers: {
            "Content-Type": "multipart/form-data",
          },
        },
      );

      return data;
    },
  });
}
