export function resolveImageUrl(path: string | null | undefined): string {
    if (!path) return '';
    return path.startsWith('http') ? path : '';
  }